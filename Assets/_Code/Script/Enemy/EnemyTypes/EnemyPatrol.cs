using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Ivayami.Player;
using Ivayami.Audio;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider), typeof(EnemySounds))]
    public class EnemyPatrol : StressEntity, IIluminatedEnemy
    {
        //[Header("Enemy Parameters")]
        [SerializeField, Min(0f)] private float _minDetectionRange;
        [SerializeField, Min(0f)] private float _detectionRange;
        [SerializeField, Min(0.01f)] private float _delayToLoseTarget;
        [SerializeField, Range(0f, 180f)] private float _visionAngle = 60f;
        [SerializeField] private Vector3 _visionOffset;
        [SerializeField, Min(0f)] private float _delayBetweenPatrolPoints;
        [SerializeField, Min(0f)] private float _delayToStopSearchTarget;
        [SerializeField, Min(0f)] private float _delayToFinishTargetSearch;
        [SerializeField, Min(.02f)] private float _behaviourTickFrequency = .5f;
        [SerializeField, Min(0f)] private float _stressIncreaseOnTargetDetected;
        [SerializeField, Min(0f)] private float _stressIncreaseWhileChasing;
        [SerializeField, Min(0f)] private float _stressMaxWhileChasing;
        [SerializeField, Min(0f)] private float _chaseSpeed;
        [SerializeField, Min(0f)] private float _minDetectionRangeInChase;
        [SerializeField] private bool _startActive;
        [SerializeField] private bool _goToLastTargetPosition;
        [SerializeField] private bool _loseTargetWhenHidden = true;
        [SerializeField] private bool _attackTarget;
        [SerializeField] private HitboxInfo[] _attackAreaInfos;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private LayerMask _blockVisionLayer;
        [SerializeField] private Vector3[] _patrolPoints;

        //[Header("Enemy Debug")]
        [SerializeField] private bool _debugLogsEnemyPatrol;
#if UNITY_EDITOR
        [SerializeField] private bool _drawMinDistance;
        [SerializeField] private Color _minDistanceAreaColor = Color.yellow;
        [SerializeField] private bool _drawMinDistanceInChase;
        [SerializeField] private Color _minDistanceInChaseAreaColor = Color.yellow;
        [SerializeField] private bool _drawDetectionRange;
        [SerializeField] private Color _detectionRangeAreaColor = Color.red;
        [SerializeField] private bool _drawPatrolPoints;
        [SerializeField] private Color _patrolPointsColor = Color.black;
        [SerializeField, Tooltip("if value in NavMeshAgent is 0, the final distance will be collider radius + 0.2")] private bool _drawStoppingDistance;
        [SerializeField, Tooltip("if value in NavMeshAgent is 0, the final distance will be collider radius + 0.2")] private Color _stoppingDistanceColor = Color.green;
        [SerializeField, Min(0)] private float _patrolPointRadius = .2f;
        private Mesh _FOVMesh;
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
        private EnemyAnimator _enemyAnimator;
        private EnemySounds _enemySounds;
        private HitboxAttack _hitboxAttack;
        private CapsuleCollider _collision;
        private Collider[] _hitsCache = new Collider[1];
        private WaitForSeconds _behaviourTickDelay;
        private WaitForSeconds _betweenPatrolPointsDelay;
        private WaitForSeconds _endGoToLastTargetDelay;
        private Coroutine _rotateCoroutine;
        private Coroutine _initializeCoroutine;
        private Coroutine _chaseStressCoroutine;
        private Coroutine _behaviourCoroutine;
        private Quaternion _initialRotation;
        private Vector3 _lastTargetPosition;
        private Vector3 _initialPosition;
        private bool _isChasing;
        private bool _canChaseTarget = true;
        private bool _canWalkPath = true;
        private bool _directContactWithTarget;
        private bool _isAttacking;
        private float _currentTargetColliderSizeFactor;
        private float _chaseTargetPatience;
        private float _goToLastTargetPointPatience;
        private float _baseSpeed;
        //private int _currentAttackAnimIndex;

        public bool IsActive { get; private set; }
        public float CurrentSpeed => _navMeshAgent.speed;

        protected override void Awake()
        {
            base.Awake();
            _collision = GetComponent<CapsuleCollider>();
            _behaviourTickDelay = new WaitForSeconds(_behaviourTickFrequency);
            _betweenPatrolPointsDelay = new WaitForSeconds(_delayBetweenPatrolPoints - _behaviourTickFrequency);
            _endGoToLastTargetDelay = new WaitForSeconds(_delayToFinishTargetSearch - _behaviourTickFrequency);
            _enemyAnimator = GetComponentInChildren<EnemyAnimator>();
            _enemySounds = GetComponent<EnemySounds>();
            if (_attackTarget) _hitboxAttack = GetComponentInChildren<HitboxAttack>();

            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _baseSpeed = _navMeshAgent.speed;
            if (_navMeshAgent.stoppingDistance == 0) _navMeshAgent.stoppingDistance = _collision.radius + .2f;
        }

        private void OnEnable()
        {
            if (!_navMeshAgent.enabled && _initializeCoroutine == null) _initializeCoroutine = StartCoroutine(InitializeAgent());
            if (_startActive && _initializeCoroutine == null) StartBehaviour();
        }

        private void OnDisable()
        {
            StopBehaviour();
        }

        private IEnumerator InitializeAgent()
        {
            yield return new WaitForEndOfFrame();
            _navMeshAgent.enabled = true;
            if (_startActive) StartBehaviour();
            _initializeCoroutine = null;
        }

        [ContextMenu("Start")]
        public void StartBehaviour()
        {
            if (!IsActive)
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
                StopCoroutine(_behaviourCoroutine);
                _behaviourCoroutine = null;
                IsActive = false;
                _isChasing = false;
                StopMovement(false);
                isStressAreaActive = false;
            }
        }

        private IEnumerator BehaviourCoroutine()
        {
            float halfVisionAngle = _visionAngle / 2f;
            byte currentPatrolPointIndex = 0;
            sbyte indexFactor = 1;
            while (IsActive)
            {
                if (!_navMeshAgent.isStopped)
                {
                    _chaseTargetPatience = Mathf.Clamp(_chaseTargetPatience - _behaviourTickFrequency, 0, _delayToLoseTarget);
                    if (CheckForTarget(halfVisionAngle)) _chaseTargetPatience = _delayToLoseTarget;
                    isStressAreaActive = _chaseTargetPatience <= 0;
                    if (_canChaseTarget && _chaseTargetPatience > 0)
                    {
                        if (!_isChasing)
                        {
                            if (_debugLogsEnemyPatrol) Debug.Log("Target Detected");
                            //StopMovement(true);
                            _enemySounds.PlaySound(EnemySounds.SoundTypes.Chasing);
                            //_enemySounds.PlaySound(EnemySounds.SoundTypes.TargetDetected, () =>
                            //{
                            //    _enemySounds.PlaySound(EnemySounds.SoundTypes.Chasing);
                            //});
                            PlayerStress.Instance.AddStress(_stressIncreaseOnTargetDetected);
                            _isChasing = true;
                            if (_stressIncreaseWhileChasing > 0) _chaseStressCoroutine ??= StartCoroutine(ChaseStressCoroutine());
                            _navMeshAgent.speed = _chaseSpeed;
                            //_enemyAnimator.TargetDetected(HandleTargetDetectedAnimationEnd);
                        }
                        _navMeshAgent.SetDestination(_hitsCache[0].transform.position);                        
                        _lastTargetPosition = _hitsCache[0].transform.position;
                        if (_debugLogsEnemyPatrol) Debug.Log("Chase Target");
                        if (_attackTarget && !_isAttacking && _chaseTargetPatience == _delayToLoseTarget && Vector3.Distance(transform.position, _navMeshAgent.destination) <= _navMeshAgent.stoppingDistance + _currentTargetColliderSizeFactor)
                        {
                            //StopMovement(true);
                            //PlayerStress.Instance.AddStress(_stressIncreaseOnAttackTarget);
                            //_isChasing = false;
                            _isAttacking = true;
                            _enemyAnimator.Attack(HandleAttackAnimationEnd, OnAnimationStepChange/*, _currentAttackAnimIndex*/);
                            //_currentAttackAnimIndex = _currentAttackAnimIndex == 0 ? 1 : 0;
                            if (_debugLogsEnemyPatrol) Debug.Log("Attack Target");
                        }
                    }
                    else
                    {
                        if (_isChasing && _canChaseTarget)
                        {
                            if (_goToLastTargetPosition)
                            {
                                if (_debugLogsEnemyPatrol) Debug.Log($"Moving to last target position {_lastTargetPosition}");
                                _navMeshAgent.SetDestination(_lastTargetPosition);
                                _goToLastTargetPointPatience += _behaviourTickFrequency;
                                if (Vector3.Distance(transform.position, _lastTargetPosition) <= _navMeshAgent.stoppingDistance)
                                {
                                    if (_debugLogsEnemyPatrol) Debug.Log("Stop Movement From going to last target");
                                    _goToLastTargetPointPatience = _delayToStopSearchTarget;
                                }
                                if (_goToLastTargetPointPatience >= _delayToStopSearchTarget)
                                {
                                    StopMovement(true);
                                    _isChasing = false;
                                    _goToLastTargetPointPatience = 0;
                                    _navMeshAgent.speed = _baseSpeed;
                                    yield return _endGoToLastTargetDelay;
                                    _navMeshAgent.isStopped = false;
                                }
                                //old version without _endGoToLastTargetDelay
                                /*_navMeshAgent.SetDestination(_lastTargetPosition);
                                if (_debugLogsEnemyPatrol) Debug.Log($"Moving to last target position {_lastTargetPosition}");
                                if (Vector3.Distance(transform.position, _lastTargetPosition) <= _navMeshAgent.stoppingDistance)
                                {
                                    _goToLastTargetPointPatience += _behaviourTickFrequency;
                                    StopMovement(false);
                                    if (_debugLogsEnemyPatrol) Debug.Log("Last target point reached");
                                    if (_goToLastTargetPointPatience >= _delayBetweenPatrolPoints)
                                    {
                                        _isChasing = false;
                                        _goToLastTargetPointPatience = 0;
                                    }
                                }*/
                            }
                            else _isChasing = false;
                        }
                        else
                        {
                            if (_canWalkPath)
                            {
                                _enemySounds.PlaySound(EnemySounds.SoundTypes.IdleScreams);
                                _navMeshAgent.SetDestination(_patrolPoints[currentPatrolPointIndex] + _initialPosition);
                                if (_debugLogsEnemyPatrol) Debug.Log("Patroling");
                                if (Vector3.Distance(transform.position, _patrolPoints[currentPatrolPointIndex] + _initialPosition) <= _navMeshAgent.stoppingDistance)
                                {
                                    if (_patrolPoints.Length > 1)
                                    {
                                        StopMovement(false);
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
                        }
                    }
                    _enemyAnimator.Chasing(_isChasing);
                    _enemyAnimator.Walking(_navMeshAgent.velocity.magnitude / _navMeshAgent.speed);
                }
                yield return _behaviourTickDelay;
            }
            _behaviourCoroutine = null;
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
                if (_directContactWithTarget && _hitsCache[0].CompareTag("Player"))
                {
                    if (_debugLogsEnemyPatrol) Debug.Log($"Chasing Stress added {_stressIncreaseWhileChasing * _behaviourTickFrequency}");
                    PlayerStress.Instance.AddStress(_stressIncreaseWhileChasing * _behaviourTickFrequency, _stressMaxWhileChasing);
                }
                yield return _behaviourTickDelay;
            }
            _chaseStressCoroutine = null;
        }

        private void StopMovement(bool stopNavMeshAgent)
        {
            if (stopNavMeshAgent) _navMeshAgent.isStopped = true;
            _navMeshAgent.velocity = Vector3.zero;
            _enemyAnimator.Walking(0);
        }

        private bool CheckForTarget(float halfVisionAngle)
        {
            Vector3 rayOrigin = transform.position + _visionOffset;
            bool targetInsideRange = _detectionRange > 0 ? Physics.OverlapSphereNonAlloc(rayOrigin, _detectionRange, _hitsCache, _targetLayer, QueryTriggerInteraction.Ignore) > 0 : true;

            bool isInMinRange;
            Vector3 targetCenter = Vector3.zero;
            float currentMinRange = _isChasing ? _minDetectionRangeInChase : _minDetectionRange;
            if (_hitsCache[0])
            {
                targetCenter = _hitsCache[0].transform.position + new Vector3(0, _hitsCache[0].bounds.size.y, 0);
                isInMinRange = Vector3.Distance(targetCenter, rayOrigin) <= currentMinRange;
            }
            else isInMinRange = Physics.OverlapSphereNonAlloc(rayOrigin, currentMinRange, _hitsCache, _targetLayer, QueryTriggerInteraction.Ignore) > 0;

            if (!_hitsCache[0]) return false;

            bool isHidden = (_loseTargetWhenHidden && PlayerMovement.Instance.hidingState != PlayerMovement.HidingState.None) || !_loseTargetWhenHidden;
            bool blockingVision = Physics.Raycast(rayOrigin, (targetCenter - rayOrigin).normalized, /*out _blockHit,*/ Vector3.Distance(rayOrigin, targetCenter), _blockVisionLayer, QueryTriggerInteraction.Ignore);
            bool isInVisionAngle = Vector3.Angle(transform.forward, (targetCenter - rayOrigin).normalized) <= halfVisionAngle;
            _currentTargetColliderSizeFactor = _hitsCache[0].bounds.extents.z;

            if (_debugLogsEnemyPatrol)
                Debug.Log($"is Hidden {isHidden}, blocking vision {blockingVision}, is in Min range {isInMinRange}, target Inside Radius {targetInsideRange}, is in Vision Angle {isInVisionAngle}");
            _directContactWithTarget = !isHidden && !blockingVision && (isInMinRange || (isInVisionAngle && targetInsideRange));
            return _directContactWithTarget;
        }

        private void HandleAttackAnimationEnd()
        {
            //_navMeshAgent.isStopped = false;
            _hitboxAttack.UpdateHitbox(false, Vector3.zero, Vector3.zero, 0, 0);
            _isAttacking = false;
        }

        //private void HandleTargetDetectedAnimationEnd()
        //{
        //    _isChasing = true;
        //    _navMeshAgent.speed = _chaseSpeed;
        //    _lastTargetPosition = _hitsCache[0].transform.position;
        //    //_navMeshAgent.isStopped = false;
        //}

        private void OnAnimationStepChange(float normalizedTime)
        {
            for (int i = 0; i < _attackAreaInfos.Length; i++)
            {
                if (normalizedTime >= _attackAreaInfos[i].MinInterval && normalizedTime <= _attackAreaInfos[i].MaxInterval)
                {
                    _hitboxAttack.UpdateHitbox(true, _attackAreaInfos[i].Center, _attackAreaInfos[i].Size, _attackAreaInfos[i].StressIncreaseOnEnter, _attackAreaInfos[i].StressIncreaseOnStay);
                    return;
                }
            }
            _hitboxAttack.UpdateHitbox(false, Vector3.zero, Vector3.zero, 0, 0);
        }

        public void ChangeSpeed(float speed)
        {
            _navMeshAgent.speed = speed;
        }

        public void UpdateBehaviour(bool canWalkPath, bool canChaseTarget)
        {
            _canChaseTarget = canChaseTarget;
            _canWalkPath = canWalkPath;
        }

        public void ChangeTargetPoint(Vector3 targetPoint)
        {
            _navMeshAgent.SetDestination(targetPoint);
        }
        #region Debug
#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            bool drawMinDistance = (Application.isPlaying && !_isChasing) || !Application.isPlaying;
            bool drawMinDistanceInChase = (Application.isPlaying && _isChasing) || !Application.isPlaying;
            if (_drawDetectionRange)
            {
                _FOVMesh = DebugUtilities.CreateConeMesh(transform, _visionAngle, _detectionRange);
                Gizmos.color = _detectionRangeAreaColor;
                Gizmos.DrawMesh(_FOVMesh, transform.position + _visionOffset, Quaternion.identity);
            }
            if (_drawMinDistance && drawMinDistance)
            {
                Gizmos.color = _minDistanceAreaColor;
                Gizmos.DrawSphere(transform.position, _minDetectionRange);
            }
            if (_drawMinDistanceInChase && drawMinDistanceInChase)
            {
                Gizmos.color = _minDistanceInChaseAreaColor;
                Gizmos.DrawSphere(transform.position, _minDetectionRangeInChase);
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

        //private void OnDrawGizmos()
        //{
        //    if (_hitsCache[0])
        //    {
        //        Gizmos.color = Color.black;
        //        Vector3 targetCenter = _hitsCache[0].transform.position + new Vector3(0, _hitsCache[0].bounds.size.y, 0);
        //        Vector3 rayOrigin = transform.position + _visionOffset;
        //        Gizmos.DrawLine(rayOrigin, targetCenter /*+ (targetCenter - rayOrigin).normalized * Vector3.Distance(rayOrigin, targetCenter)*/);
        //    }
        //    if (_blockHit.collider)
        //    {
        //        Gizmos.color = Color.blue;
        //        Gizmos.DrawSphere(_blockHit.point, .1f);
        //    }
        //}

        protected override void OnValidate()
        {
            base.OnValidate();
            if (_attackAreaInfos == null) return;
            for (int i = 0; i < _attackAreaInfos.Length; i++)
            {
                if (_attackAreaInfos[i].MinInterval > _attackAreaInfos[i].MaxInterval) _attackAreaInfos[i].MinInterval = _attackAreaInfos[i].MaxInterval;
            }
            //if (_attackTarget && !_hitboxAttack)
            //{
            //    _hitboxAttack = GetComponentInChildren<HitboxAttack>();
            //    if (!_hitboxAttack)
            //    {
            //        Debug.Log("To make enemy attack please add a HitboxAttack component as child");
            //        //GameObject go = new GameObject("HitboxAttackArea");
            //        //go.transform.parent = transform;
            //        //_hitboxAttack = go.AddComponent<HitboxAttack>();
            //    }
            //}
        }
#endif
        #endregion
    }
}