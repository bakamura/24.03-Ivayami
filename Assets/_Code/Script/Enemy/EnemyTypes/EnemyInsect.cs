using Ivayami.Audio;
using Ivayami.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Ivayami.Player.Ability;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider), typeof(EnemySounds))]
    public class EnemyInsect : StressEntity, IIluminatedEnemy
    {
        [SerializeField, Min(.02f)] private float _behaviourTickFrequency = .5f;
        [SerializeField] private bool _startActive;

        [SerializeField, Min(0f)] private float _chaseSpeed;
        [SerializeField, Min(0f)] private float _minDetectionRange;
        [SerializeField, Min(0f)] private float _afterAttackCooldownDuration;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private LayerMask _blockVisionLayer;

        [SerializeField, Min(0f)] private float _delayBetweenPatrolPoints;
        [SerializeField] private Vector3[] _patrolPoints;

        [SerializeField, Min(0f)] private float _stressIncreaseWhileChasing;
        [SerializeField, Min(0f)] private float _stressMaxWhileChasing;
        [SerializeField, Min(0f)] private float _fuelRemoveFromLantern;
        [SerializeField] private HitboxInfo[] _attackAreaInfos;

        //[Header("Enemy Debug")]
        [SerializeField] private bool _debugLogsEnemyPatrol;
#if UNITY_EDITOR
        [SerializeField] private bool _drawMinDistance;
        [SerializeField] private Color _minDistanceAreaColor = Color.yellow;
        [SerializeField] private bool _drawPatrolPoints;
        [SerializeField] private Color _patrolPointsColor = Color.black;
        [SerializeField, Min(0)] private float _patrolPointRadius = .2f;
        [SerializeField, Tooltip("if value in NavMeshAgent is 0, the final distance will be collider radius + 0.2")] private bool _drawStoppingDistance;
        [SerializeField, Tooltip("if value in NavMeshAgent is 0, the final distance will be collider radius + 0.2")] private Color _stoppingDistanceColor = Color.green;
        //private RaycastHit _blockHit;
#endif

        private NavMeshAgent _navMeshAgent
        {
            get
            {
                if (!m_navMeshAgent) m_navMeshAgent = GetComponent<NavMeshAgent>();
                return m_navMeshAgent;
            }
        }
        private NavMeshAgent m_navMeshAgent;
        private EnemyAnimator _enemyAnimator
        {
            get
            {
                if (!m_enemyAnimator) m_enemyAnimator = GetComponentInChildren<EnemyAnimator>();
                return m_enemyAnimator;
            }
        }
        private EnemyAnimator m_enemyAnimator;
        private EnemySounds _enemySounds;
        private IlluminatedEnemyDetector _lightDetector;
        private CapsuleCollider _collision;
        private Collider[] _hitsCache = new Collider[1];
        private WaitForSeconds _behaviourTickDelay;
        private WaitForSeconds _betweenPatrolPointsDelay;
        private Coroutine _rotateCoroutine;
        private Coroutine _initializeCoroutine;
        private Coroutine _chaseStressCoroutine;
        private Coroutine _behaviourCoroutine;
        private Coroutine _attackCooldownCoroutine;
        private Quaternion _initialRotation;
        private Vector3 _initialPosition;
        private bool _isChasing;
        private bool _directContactWithTarget;
        private bool _isAttacking;
        private float _currentTargetColliderSizeFactor;
        private float _baseSpeed;
        private float _baseStoppingDistance;
        private float _currentChasePatience;

        public bool IsActive { get; private set; }
        public float CurrentSpeed => _navMeshAgent.speed;
        public Vector3 CurrentPosition => transform.position;

        protected override void Awake()
        {
            base.Awake();
            _collision = GetComponent<CapsuleCollider>();
            _behaviourTickDelay = new WaitForSeconds(_behaviourTickFrequency);
            _betweenPatrolPointsDelay = new WaitForSeconds(_delayBetweenPatrolPoints - _behaviourTickFrequency);
            _enemySounds = GetComponent<EnemySounds>();
            _lightDetector = GetComponentInChildren<IlluminatedEnemyDetector>();
            _lightDetector.onIlluminatedByLantern.AddListener(HandleIluminatedByLantern);

            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _baseSpeed = _navMeshAgent.speed;
            if (_navMeshAgent.stoppingDistance == 0) _navMeshAgent.stoppingDistance = _collision.radius + .2f;
            _baseStoppingDistance = _navMeshAgent.stoppingDistance;
        }

        private void OnEnable()
        {
            if (!_navMeshAgent.enabled && _initializeCoroutine == null)
                _initializeCoroutine = StartCoroutine(InitializeAgent());
            if (_startActive && _initializeCoroutine == null)
                StartBehaviour();
        }

        private void OnDisable()
        {
            StopBehaviour();
        }

        private IEnumerator InitializeAgent()
        {
            yield return null;
            _navMeshAgent.enabled = true;
            if (_startActive) StartBehaviour();
            _initializeCoroutine = null;
        }

        [ContextMenu("Start")]
        public void StartBehaviour()
        {
            if (!_navMeshAgent.isOnNavMesh)
            {
                //Debug.LogWarning($"Enemy {name} of type {typeof(EnemyPatrol)} is not in a navmesh");
                return;
            }
            if (!IsActive && _navMeshAgent.isOnNavMesh)
            {
                if (!_navMeshAgent.enabled && _initializeCoroutine == null)
                {
                    _initializeCoroutine = StartCoroutine(InitializeAgent());
                    return;
                }
                IsActive = true;
                _navMeshAgent.isStopped = false;
                _behaviourCoroutine = StartCoroutine(BehaviourCoroutine());
            }
        }

        [ContextMenu("Stop")]
        public void StopBehaviour()
        {
            if (IsActive)
            {
                if (_behaviourCoroutine != null)
                {
                    StopCoroutine(_behaviourCoroutine);
                    _behaviourCoroutine = null;
                }
                if (_attackCooldownCoroutine != null)
                {
                    StopCoroutine(_attackCooldownCoroutine);
                    _attackCooldownCoroutine = null;
                }
                IsActive = false;
                _isChasing = false;
                UpdateMovement(false);
                isStressAreaActive = false;
            }
        }

        private IEnumerator BehaviourCoroutine()
        {
            byte currentPatrolPointIndex = 0;
            sbyte indexFactor = 1;
            bool inAttackCooldown;
            while (IsActive)
            {
                inAttackCooldown = _attackCooldownCoroutine != null;
                if (CheckForTarget() && !_isAttacking && !inAttackCooldown)
                {
                    _navMeshAgent.SetDestination(transform.position);
                    UpdateMovement(true);
                    isStressAreaActive = false;
                    _isAttacking = true;
                    _isChasing = false;
                    PlayerMovement.Instance.ToggleMovement(nameof(EnemyInsect) + gameObject.name, false);
                    //_currentAttackAnimIndex = _currentAttackAnimIndex == 0 ? 1 : 0;
                    _enemySounds.PlaySound(EnemySounds.SoundTypes.Attack);
                    _enemyAnimator.Attack(HandleAttackAnimationEnd, OnAnimationStepChange/*, _currentAttackAnimIndex*/);

                    if (LanternRef.Instance.AbilityAquired())
                    {
                        LanternRef.Instance.Lantern.Fill(-_fuelRemoveFromLantern);
                        LanternRef.Instance.Lantern.ForceTurnOff();
                    }
                    if (PlayerActions.Instance.CheckAbility(typeof(Lantern), out PlayerAbility ability))
                    {
                        Lantern lantern = (Lantern)ability;
                        lantern.Fill(-_fuelRemoveFromLantern);
                    }

                    PlayerMovement.Instance.SetTargetAngle(Quaternion.LookRotation(transform.position - _hitsCache[0].transform.position).eulerAngles.y);
                    transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(_hitsCache[0].transform.position - transform.position));

                    if (_debugLogsEnemyPatrol) Debug.Log("Attack Target");
                    StopBehaviour();
                }
                else if (_isChasing && !inAttackCooldown)
                {
                    isStressAreaActive = false;
                    if (_debugLogsEnemyPatrol) Debug.Log("Chasing");
                    if (_stressIncreaseWhileChasing > 0) _chaseStressCoroutine ??= StartCoroutine(ChaseStressCoroutine());
                    _navMeshAgent.speed = _chaseSpeed;
                    _navMeshAgent.SetDestination(PlayerMovement.Instance.transform.position);
                    //if (Vector3.Distance(PlayerMovement.Instance.transform.position, transform.position) <= _navMeshAgent.stoppingDistance)
                    //{
                    //    _currentChasePatience -= _behaviourTickFrequency;
                    //    if (_currentChasePatience <= 0) _isChasing = false;
                    //}
                    //else _currentChasePatience = _durationInCurrentSoundTarget;
                }
                else
                {
                    isStressAreaActive = true;
                    _navMeshAgent.speed = _baseSpeed;
                    _enemySounds.PlaySound(EnemySounds.SoundTypes.IdleScreams);
                    _navMeshAgent.SetDestination(_patrolPoints[currentPatrolPointIndex] + _initialPosition);
                    if (_debugLogsEnemyPatrol) Debug.Log("Patroling");
                    if (Vector3.Distance(transform.position, _patrolPoints[currentPatrolPointIndex] + _initialPosition) <= _navMeshAgent.stoppingDistance)
                    {
                        if (_patrolPoints.Length > 1)
                        {
                            _enemyAnimator.Walking(0);
                            yield return _betweenPatrolPointsDelay;
                            currentPatrolPointIndex = (byte)(currentPatrolPointIndex + indexFactor);
                            if (currentPatrolPointIndex == _patrolPoints.Length - 1) indexFactor = -1;
                            else if (currentPatrolPointIndex == 0 && indexFactor == -1) indexFactor = 1;
                            if (_debugLogsEnemyPatrol) Debug.Log($"Change Patrol Point to {currentPatrolPointIndex}");
                        }
                        else
                        {
                            if (transform.rotation != _initialRotation && _rotateCoroutine == null) _rotateCoroutine = StartCoroutine(RotateCoroutine());
                        }
                    }
                }
                _enemyAnimator.Chasing(_isChasing);
                _enemyAnimator.Walking(_navMeshAgent.velocity.magnitude/* / _navMeshAgent.speed*/);
                yield return _behaviourTickDelay;
                _behaviourCoroutine = null;
            }
        }

        private IEnumerator RotateCoroutine()
        {
            WaitForFixedUpdate delay = new WaitForFixedUpdate();
            while (transform.rotation != _initialRotation)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _initialRotation, _navMeshAgent.angularSpeed * Time.fixedDeltaTime);
                yield return delay;
            }
            _rotateCoroutine = null;
        }

        private IEnumerator ChaseStressCoroutine()
        {
            while (_isChasing)
            {
                if (_debugLogsEnemyPatrol) Debug.Log($"Chasing Stress added {_stressIncreaseWhileChasing * _behaviourTickFrequency}");
                PlayerStress.Instance.AddStress(_stressIncreaseWhileChasing * _behaviourTickFrequency, _stressMaxWhileChasing);
                yield return _behaviourTickDelay;
            }
            _chaseStressCoroutine = null;
        }

        private IEnumerator AttackCooldownCoroutine()
        {
            yield return new WaitForSeconds(_afterAttackCooldownDuration);
            _attackCooldownCoroutine = null;
        }

        private void UpdateMovement(bool isStoped)
        {
            if (!_navMeshAgent.enabled) return;
            if (_navMeshAgent.isOnNavMesh) _navMeshAgent.isStopped = isStoped;
            if (isStoped) _navMeshAgent.velocity = Vector3.zero;
            _enemyAnimator.Walking(isStoped ? 0 : _navMeshAgent.velocity.magnitude / _navMeshAgent.speed);
        }

        private bool CheckForTarget()
        {
            Vector3 rayOrigin = transform.position;
            bool targetInsideRange = _minDetectionRange > 0 ? Physics.OverlapSphereNonAlloc(rayOrigin, _minDetectionRange, _hitsCache, _targetLayer, QueryTriggerInteraction.Ignore) > 0 : true;

            if (!_hitsCache[0]) return false;
            Vector3 targetCenter = _hitsCache[0].transform.position + new Vector3(0, _hitsCache[0].bounds.size.y, 0);

            bool blockingVision = _blockVisionLayer.value == 0 ? false : Physics.Raycast(rayOrigin, (targetCenter - rayOrigin).normalized, /*out _blockHit,*/ Vector3.Distance(rayOrigin, targetCenter), _blockVisionLayer, QueryTriggerInteraction.Ignore);
            _currentTargetColliderSizeFactor = _hitsCache[0].bounds.extents.z;

            if (_debugLogsEnemyPatrol)
                Debug.Log($"blocking vision {blockingVision}, target Inside Radius {targetInsideRange}");
            _directContactWithTarget = !blockingVision && targetInsideRange;
            _navMeshAgent.stoppingDistance = _directContactWithTarget ? _collision.radius + _currentTargetColliderSizeFactor + .1f : _baseStoppingDistance;
            return _directContactWithTarget;
        }

        private void HandleAttackAnimationEnd()
        {
            for (int i = 0; i < _attackAreaInfos.Length; i++)
            {
                _attackAreaInfos[i].Hitbox.UpdateHitbox(false, Vector3.zero, Vector3.zero, 0, 0, PlayerAnimation.DamageAnimation.None);
            }
            _isAttacking = false;
            PlayerMovement.Instance.ToggleMovement(nameof(EnemyInsect) + gameObject.name, true);
            _attackCooldownCoroutine = StartCoroutine(AttackCooldownCoroutine());
            StartBehaviour();
        }

        private void OnAnimationStepChange(float normalizedTime)
        {
            int currentAnimIndex = _enemyAnimator.GetCurrentAttackAnimIndex();
            for (int i = 0; i < _attackAreaInfos.Length; i++)
            {
                if (_attackAreaInfos[i].AnimationIndex == currentAnimIndex && normalizedTime >= _attackAreaInfos[i].MinInterval && normalizedTime <= _attackAreaInfos[i].MaxInterval)
                {
                    _attackAreaInfos[i].Hitbox.UpdateHitbox(true, _attackAreaInfos[i].Center, _attackAreaInfos[i].Size, _attackAreaInfos[i].StressIncreaseOnEnter, _attackAreaInfos[i].StressIncreaseOnStay, _attackAreaInfos[i].DamageType);
                    //return;
                }
                else _attackAreaInfos[i].Hitbox.UpdateHitbox(false, Vector3.zero, Vector3.zero, 0, 0, PlayerAnimation.DamageAnimation.None);
            }
            //_attackAreaInfos[i].Hitbox.UpdateHitbox(false, Vector3.zero, Vector3.zero, 0, 0);
        }

        private void HandleIluminatedByLantern(bool isIluminated)
        {
            _isChasing = isIluminated;
        }

        #region Debug
#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            bool drawMinDistance = (Application.isPlaying && !_isChasing) || !Application.isPlaying;
            if (_drawMinDistance && drawMinDistance)
            {
                Gizmos.color = _minDistanceAreaColor;
                Gizmos.DrawSphere(transform.position, _minDetectionRange);
            }
            if (_drawPatrolPoints)
            {
                Vector3 pos = _initialPosition != Vector3.zero ? _initialPosition : transform.position;
                Gizmos.color = _patrolPointsColor;
                int i;
                for (i = 0; i < _patrolPoints.Length; i++)
                {
                    Gizmos.DrawSphere(pos + _patrolPoints[i], _patrolPointRadius);
                    UnityEditor.Handles.Label(pos + _patrolPoints[i] + new Vector3(0, _patrolPointRadius * 2, 0), i.ToString());
                }
                for (i = 0; i < _patrolPoints.Length - 1; i++)
                {
                    Gizmos.DrawLine(pos + _patrolPoints[i], pos + _patrolPoints[Mathf.Clamp(i + 1, 0, _patrolPoints.Length - 1)]);
                }
            }
            if (_drawStoppingDistance)
            {
                Gizmos.color = _stoppingDistanceColor;
                Gizmos.DrawLine(transform.position, transform.position + transform.right * _navMeshAgent.stoppingDistance);
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (_attackAreaInfos == null) return;
            for (int i = 0; i < _attackAreaInfos.Length; i++)
            {
                if (_attackAreaInfos[i].MinInterval > _attackAreaInfos[i].MaxInterval) _attackAreaInfos[i].MinInterval = _attackAreaInfos[i].MaxInterval;
            }
        }

        public void ChangeSpeed(float speed)
        {
            
        }

        public void UpdateBehaviour(bool canWalkPath, bool canChaseTarget, bool isStopped, bool forceTargetDetect)
        {
            
        }

        public void ChangeTargetPoint(Vector3 targetPoint)
        {
            
        }
#endif
        #endregion
    }
}