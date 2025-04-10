using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Ivayami.Player;
using Ivayami.Audio;
using System;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider), typeof(EnemySounds))]
    public class EnemyAngel : StressEntity, IIluminatedEnemy, IEnemyWalkArea
    {
        [SerializeField, Min(.02f)] private float _behaviourTickFrequency = .5f;
        [SerializeField] private bool _startActive;

        [SerializeField, Min(0f)] private float _chaseSpeed;
        [SerializeField, Min(0f)] private float _detectionRange;
        [SerializeField, Min(0f)] private float _minDetectionRange;
        [SerializeField, Min(0f)] private float _minDetectionRangeInChase;
        [SerializeField, Range(0f, 180f)] private float _visionAngle = 60f;
        [SerializeField] private Vector3 _visionOffset;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private LayerMask _blockVisionLayer;

        [SerializeField] private bool _goToLastTargetPosition;
        [SerializeField] private bool _loseTargetWhenHidden = true;
        [SerializeField] private PatrolTypes _patrolType;
        [SerializeField, Min(0.01f)] private float _delayToLoseTarget;
        [SerializeField, Min(0f)] private float _delayToStopSearchTarget;
        [SerializeField, Min(0f)] private float _delayToFinishTargetSearch;
        [SerializeField, Min(0f)] private float _delayBetweenPatrolPoints;
        [SerializeField] private EnemyWalkArea _fixedWalkArea;
        [SerializeField] private Vector3[] _patrolPoints;

        //[SerializeField, Min(0f)] private float _stressIncreaseOnTargetDetected;
        [SerializeField, Min(0f)] private float _stressIncreaseWhileChasing;
        [SerializeField, Min(0f)] private float _stressMaxWhileChasing;
        [SerializeField, Min(0f)] private float _distanceToLeapAttack;
        [SerializeField, Min(0f)] private float _distanceToFogAttack;
        [SerializeField, Min(0f)] private float _leapAttackRange;
        [SerializeField, Min(0f)] private float _leapAttackDuration;
        [SerializeField, Range(0f, 1f)] private float _startLeapMovementInterval;
        [SerializeField, Min(1f)] private float _leapAttackJumpHeight;
        [SerializeField] private AnimationCurve _leapAttackHeightCurve;
        [SerializeField, Min(0f)] private float _leapAttackPredictDistance;
        [SerializeField, Min(0f)] private float _fallDuration;
        [SerializeField] private HitboxInfo[] _attackAreaInfos;

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
        [SerializeField, Min(0)] private float _patrolPointRadius = .2f;
        [SerializeField, Tooltip("if value in NavMeshAgent is 0, the final distance will be collider radius + 0.2")] private bool _drawStoppingDistance;
        [SerializeField, Tooltip("if value in NavMeshAgent is 0, the final distance will be collider radius + 0.2")] private Color _stoppingDistanceColor = Color.green;
        [SerializeField] private bool _drawFogAttackDistance;
        [SerializeField] private Color _fogAttackColor;
        [SerializeField] private bool _drawLeapAttackDistance;
        [SerializeField] private Color _leapAttackColor;
        [SerializeField] private bool _drawPredictPosition;
        [SerializeField] private Color _predictPositionColor;
        [SerializeField] private bool _drawLeapAttackRange;
        [SerializeField] private Color _leapAttackRangeColor;
        private Mesh _FOVMesh;
        //private RaycastHit _blockHit;
#endif

        private enum PatrolTypes
        {
            BasicPatrol,
            EnemyWalkArea
        }
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
        private CapsuleCollider _collision
        {
            get
            {
                if (!m_collision) m_collision = GetComponent<CapsuleCollider>();
                return m_collision;
            }
        }
        private CapsuleCollider m_collision;
        private Collider[] _hitsCache = new Collider[1];
        private WaitForSeconds _behaviourTickDelay;
        private WaitForSeconds _betweenPatrolPointsDelay;
        private WaitForSeconds _endGoToLastTargetDelay;
        private Coroutine _rotateCoroutine;
        private Coroutine _initializeCoroutine;
        private Coroutine _chaseStressCoroutine;
        private Coroutine _behaviourCoroutine;
        private Coroutine _leapCoroutine;
        private Coroutine _fallCoroutine;
        private EnemyWalkArea _currentWalkArea;
        private EnemyMovementData _currentMovementData;
        private Quaternion _initialRotation;
        private Vector3 _lastTargetPosition;
        private Vector3 _initialPosition;
        private Vector3 _initialLeapPosition;
        private bool _isChasing;
        private bool _canChaseTarget = true;
        private bool _canWalkPath = true;
        private bool _directContactWithTarget;
        private bool _isAttacking;
        private bool _isInLeapAnimation;
        private float _currentTargetColliderSizeFactor;
        private float _chaseTargetPatience;
        private float _goToLastTargetPointPatience;
        private float _baseSpeed;
        private float _baseStoppingDistance;
        private float _currentLeapAnimationTime;
        private float _predictDistance => _navMeshAgent.radius + _currentTargetColliderSizeFactor + _leapAttackPredictDistance;

        public bool IsActive { get; private set; }
        public float CurrentSpeed => _navMeshAgent.speed;

        public bool CanChangeWalkArea => !_fixedWalkArea;

        public int ID => gameObject.GetInstanceID();

        protected override void Awake()
        {
            base.Awake();
            _behaviourTickDelay = new WaitForSeconds(_behaviourTickFrequency);
            _betweenPatrolPointsDelay = new WaitForSeconds(_delayBetweenPatrolPoints - _behaviourTickFrequency);
            _endGoToLastTargetDelay = new WaitForSeconds(_delayToFinishTargetSearch - _behaviourTickFrequency);
            _enemySounds = GetComponent<EnemySounds>();

            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _baseSpeed = _navMeshAgent.speed;

            if (_navMeshAgent.stoppingDistance == 0) _navMeshAgent.stoppingDistance = _collision.radius + .2f;
            _baseStoppingDistance = _navMeshAgent.stoppingDistance;

            if (_fixedWalkArea)
            {
                SetMovementData(_fixedWalkArea.MovementData);
                SetWalkArea(_fixedWalkArea);
                _fixedWalkArea.AddEnemyToArea(this, gameObject.name);
            }
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
                //Debug.LogWarning($"Enemy {name} of type {typeof(EnemyAngel)} is not in a navmesh");
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
                StopCoroutine(_behaviourCoroutine);
                _behaviourCoroutine = null;
                if (_leapCoroutine != null)
                {
                    StopCoroutine(_leapCoroutine);
                    _leapCoroutine = null;
                    transform.position = _initialLeapPosition;
                    _navMeshAgent.enabled = true;
                }
                if (_fallCoroutine != null)
                {
                    StopCoroutine(_fallCoroutine);
                    _fallCoroutine = null;
                }
                IsActive = false;
                _isChasing = false;
                UpdateMovement(false);
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
                if (_navMeshAgent.enabled && !_navMeshAgent.isStopped && _fallCoroutine == null)
                {
                    _chaseTargetPatience = Mathf.Clamp(_chaseTargetPatience - _behaviourTickFrequency, 0, _delayToLoseTarget);
                    if (CheckForTarget(halfVisionAngle)) _chaseTargetPatience = _delayToLoseTarget;
                    isStressAreaActive = _chaseTargetPatience <= 0;
                    if (_canChaseTarget && _chaseTargetPatience > 0 && _hitsCache[0])
                    {
                        if (!_isChasing)
                        {
                            if (_debugLogsEnemyPatrol) Debug.Log("Target Detected");
                            _enemySounds.PlaySound(EnemySounds.SoundTypes.Chasing);
                            _isChasing = true;
                            if (_stressIncreaseWhileChasing > 0) _chaseStressCoroutine ??= StartCoroutine(ChaseStressCoroutine());
                            SetToChaseSpeed();
                        }
                        _navMeshAgent.SetDestination(_hitsCache[0].transform.position);
                        _lastTargetPosition = _hitsCache[0].transform.position;
                        if (_debugLogsEnemyPatrol) Debug.Log("Chase Target");
                        if (!_isAttacking && _chaseTargetPatience == _delayToLoseTarget)
                        {
                            Attack();
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
                                    UpdateMovement(true);
                                    _isChasing = false;
                                    _goToLastTargetPointPatience = 0;
                                    SetToWalkSpeed();
                                    yield return _endGoToLastTargetDelay;
                                    UpdateMovement(false);
                                }
                            }
                            else _isChasing = false;
                        }
                        else
                        {
                            if (_canWalkPath)
                            {
                                SetToWalkSpeed();
                                _enemySounds.PlaySound(EnemySounds.SoundTypes.IdleScreams);
                                if (_debugLogsEnemyPatrol) Debug.Log("Patroling");
                                switch (_patrolType)
                                {
                                    case PatrolTypes.BasicPatrol:
                                        _navMeshAgent.SetDestination(_patrolPoints[currentPatrolPointIndex] + _initialPosition);
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
                                                if (transform.rotation != _initialRotation && _rotateCoroutine == null) _rotateCoroutine = StartCoroutine(RotateCoroutine(_initialRotation));
                                            }
                                        }
                                        break;
                                    case PatrolTypes.EnemyWalkArea:
                                        if (_currentWalkArea && _currentWalkArea.GetCurrentPoint(ID, out EnemyWalkArea.EnemyData point))
                                        {
                                            if (Vector3.Distance(transform.position, point.Point.Position) <= _navMeshAgent.stoppingDistance)
                                            {
                                                _navMeshAgent.velocity = Vector3.zero;
                                                _enemyAnimator.Walking(0);
                                                yield return new WaitForSeconds(point.Point.DelayToNextPoint);
                                                _navMeshAgent.SetDestination(_currentWalkArea.GoToNextPoint(ID).Point.Position);
                                                if (_debugLogsEnemyPatrol)
                                                {
                                                    _currentWalkArea.GetCurrentPoint(ID, out point);
                                                    Debug.Log($"Change Patrol Point to {point.CurrentPointIndex}");
                                                }
                                            }
                                            else
                                            {
                                                _navMeshAgent.SetDestination(point.Point.Position);
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    _enemyAnimator.Chasing(_isChasing);
                    _enemyAnimator.Walking(_navMeshAgent.velocity.magnitude /*/ _navMeshAgent.speed*/);
                }
                yield return _behaviourTickDelay;
            }
            _behaviourCoroutine = null;
        }

        private IEnumerator RotateCoroutine(Quaternion finalRotation)
        {
            WaitForFixedUpdate delay = new WaitForFixedUpdate();
            while (transform.rotation != finalRotation)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, finalRotation, _navMeshAgent.angularSpeed * Time.fixedDeltaTime);
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

        private void UpdateMovement(bool isStoped)
        {
            if (!_navMeshAgent.enabled) return;
            if (_navMeshAgent.isOnNavMesh) _navMeshAgent.isStopped = isStoped;
            if (isStoped) _navMeshAgent.velocity = Vector3.zero;
            _enemyAnimator.Walking(isStoped ? 0 : _navMeshAgent.velocity.magnitude / _navMeshAgent.speed);
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

            if (!_hitsCache[0])
            {
                _directContactWithTarget = false;
                return false;
            }

            bool isHidden = (_loseTargetWhenHidden && PlayerMovement.Instance.hidingState != PlayerMovement.HidingState.None) || !_loseTargetWhenHidden;
            bool blockingVision = Physics.Raycast(rayOrigin, (targetCenter - rayOrigin).normalized, /*out _blockHit,*/ Vector3.Distance(rayOrigin, targetCenter), _blockVisionLayer, QueryTriggerInteraction.Ignore);
            bool isInVisionAngle = Vector3.Angle(transform.forward, (targetCenter - rayOrigin).normalized) <= halfVisionAngle;
            _currentTargetColliderSizeFactor = _hitsCache[0].bounds.extents.z;

            if (_debugLogsEnemyPatrol)
                Debug.Log($"is Hidden {isHidden}, blocking vision {blockingVision}, is in Min range {isInMinRange}, target Inside Radius {targetInsideRange}, is in Vision Angle {isInVisionAngle}");
            _directContactWithTarget = !isHidden && !blockingVision && (isInMinRange || (isInVisionAngle && targetInsideRange));
            _navMeshAgent.stoppingDistance = _directContactWithTarget ? _collision.radius + _currentTargetColliderSizeFactor + .1f : _baseStoppingDistance;
            return _directContactWithTarget;
        }

        #region LeapAttack
        /// <summary>
        ///  0 = Fog attack, 1 = Leap attack
        /// </summary>
        /// <param name="attackIndex"></param>
        private void Attack()
        {
            //if (_isAttacking) return;
            bool distanceToFogAttack = Vector3.Distance(transform.position, _hitsCache[0].transform.position) <= _distanceToFogAttack;
            bool distanceToLeapAttack = Vector3.Distance(transform.position, _hitsCache[0].transform.position) <= _distanceToLeapAttack;
            _isAttacking = distanceToFogAttack || distanceToLeapAttack;
            if (_isAttacking)
            {
                _navMeshAgent.isStopped = true;
                int attackIndex = 0;
                if (distanceToFogAttack) attackIndex = 0;
                else if (distanceToLeapAttack)
                {
                    attackIndex = 1;
                    Vector3 dir = _hitsCache[0].transform.position - transform.position;
                    dir = new Vector3(dir.x, 0, dir.z);
                    _rotateCoroutine = StartCoroutine(RotateCoroutine(Quaternion.LookRotation(dir.normalized)));
                }
                _enemyAnimator.Attack(HandleAttackAnimationEnd, OnAnimationStepChange, attackIndex);
                //Debug.Log("AttackAnimReady");
                if (_debugLogsEnemyPatrol) Debug.Log("Attack Target");
            }
        }

        private void HandleAttackAnimationEnd()
        {
            for (int i = 0; i < _attackAreaInfos.Length; i++)
            {
                _attackAreaInfos[i].Hitbox.UpdateHitbox(false, Vector3.zero, Vector3.zero, 0, 0);
            }
            _isInLeapAnimation = false;
            //Debug.Log("EndAttack");
            if (!_canChaseTarget && !_canWalkPath && _leapCoroutine != null && _fallCoroutine == null)
            {
                StopCoroutine(_leapCoroutine);
                _leapCoroutine = null;
                _fallCoroutine = StartCoroutine(FallCoroutine());
            }
            else
            {
                _navMeshAgent.enabled = true;
                _navMeshAgent.isStopped = false;
            }
            _isAttacking = false;
        }

        private void OnAnimationStepChange(float normalizedTime)
        {
            int currentAnimIndex = _enemyAnimator.GetCurrentAttackAnimIndex();
            _currentLeapAnimationTime = normalizedTime;
            for (int i = 0; i < _attackAreaInfos.Length; i++)
            {
                if (!_attackAreaInfos[i].WillInterpolateAnySize() && _attackAreaInfos[i].AnimationIndex == currentAnimIndex && normalizedTime >= _attackAreaInfos[i].MinInterval && normalizedTime <= _attackAreaInfos[i].MaxInterval)
                {
                    _attackAreaInfos[i].Hitbox.UpdateHitbox(true, _attackAreaInfos[i].Center, _attackAreaInfos[i].Size, _attackAreaInfos[i].StressIncreaseOnEnter, _attackAreaInfos[i].StressIncreaseOnStay);
                }
                else
                {
                    //deactivates all hitboxes that dont lerp size
                    if (!_attackAreaInfos[i].WillInterpolateAnySize()) _attackAreaInfos[i].Hitbox.UpdateHitbox(false, Vector3.zero, Vector3.zero, 0, 0);
                }
            }
            if (currentAnimIndex == 1 && normalizedTime >= _startLeapMovementInterval && !_isInLeapAnimation)
            {
                if (!CheckForTarget(_visionAngle / 2f))
                {
                    _enemyAnimator.ForcePlayState("idle");
                    HandleAttackAnimationEnd();
                    return;
                }
                _isInLeapAnimation = true;
                _navMeshAgent.enabled = false;
                _initialLeapPosition = transform.position;
                _leapCoroutine = StartCoroutine(LeapCoroutine());
            }
            //_attackAreaInfos[i].Hitbox.UpdateHitbox(false, Vector3.zero, Vector3.zero, 0, 0);
        }

        private IEnumerator LeapCoroutine()
        {
            WaitForFixedUpdate delay = new WaitForFixedUpdate();
            float count = 0;
            // this is a direction that points to the enemy
            Vector3 dir = (_initialLeapPosition - _hitsCache[0].transform.position).normalized;
            Vector3 finalPos = GetFinalPos();
            Vector3 colliderSize;
            transform.rotation = Quaternion.LookRotation(-dir);

            while (count < 1)
            {
                count += Time.fixedDeltaTime / _leapAttackDuration;
                transform.position = new Vector3(Mathf.Lerp(_initialLeapPosition.x, finalPos.x, count), Mathf.Lerp(_initialLeapPosition.y, finalPos.y + _leapAttackJumpHeight, _leapAttackHeightCurve.Evaluate(count)), Mathf.Lerp(_initialLeapPosition.z, finalPos.z, count));
                for (int i = 0; i < _attackAreaInfos.Length; i++)
                {
                    if (_attackAreaInfos[i].WillInterpolateAnySize() && _attackAreaInfos[i].AnimationIndex == 1 &&
                        _currentLeapAnimationTime >= _attackAreaInfos[i].MinInterval && _currentLeapAnimationTime <= _attackAreaInfos[i].MaxInterval)
                    {
                        colliderSize = new Vector3(_attackAreaInfos[i].Size.x, _attackAreaInfos[i].Size.y, Vector3.Distance(transform.position, _initialLeapPosition));
                        _attackAreaInfos[i].Hitbox.UpdateHitbox(true, _attackAreaInfos[i].Center, colliderSize, _attackAreaInfos[i].StressIncreaseOnEnter, _attackAreaInfos[i].StressIncreaseOnStay);
                    }
                    else
                    {
                        //deactivate all hitboxes that lerp size
                        if (_attackAreaInfos[i].WillInterpolateAnySize()) _attackAreaInfos[i].Hitbox.UpdateHitbox(false, Vector3.zero, Vector3.zero, 0, 0);
                    }
                }
                yield return delay;
            }
            _leapCoroutine = null;

            Vector3 GetFinalPos()
            {
                Vector3 finalPos;
                Vector3 currentPlayerMovement = new Vector3(PlayerMovement.Instance.MovementDirection.x, 0, PlayerMovement.Instance.MovementDirection.z);
                if (Vector3.Distance(transform.position, _hitsCache[0].transform.position) <= _leapAttackRange)
                {
                    //float halfExtent = _navMeshAgent.radius * .75f;
                    float halfHeight = _navMeshAgent.height * .5f;
                    //Physics.SphereCast(new Ray(_hitsCache[0].transform.position + new Vector3(0, _navMeshAgent.height * .3f, 0), _currentPlayerMovement.normalized), _collision.radius * .5f, _collision.radius + _currentTargetColliderSizeFactor, _blockVisionLayer)
                    //Physics.BoxCast(_hitsCache[0].transform.position + new Vector3(0, halfHeight, 0), new Vector3(halfExtent, halfHeight, halfExtent), currentPlayerMovement.normalized, Quaternion.identity, _predictDistance, _blockVisionLayer)
                    //Physics.Raycast(new Ray(_hitsCache[0].transform.position + new Vector3(0, halfHeight, 0), currentPlayerMovement.normalized), _predictDistance, _blockVisionLayer);
                    if (Physics.Raycast(new Ray(_hitsCache[0].transform.position + new Vector3(0, halfHeight, 0), currentPlayerMovement.normalized), _predictDistance, _blockVisionLayer))
                    {
                        finalPos = _hitsCache[0].transform.position + dir * (_navMeshAgent.radius + _currentTargetColliderSizeFactor);
                        if (_debugLogsEnemyPatrol) Debug.Log("Position Blocked, Leap attack close to player");
                    }
                    else
                    {
                        finalPos = _hitsCache[0].transform.position + currentPlayerMovement.normalized * _predictDistance;
                        if (_debugLogsEnemyPatrol) Debug.Log("Leap attack predict position");
                    }
                }
                else
                {
                    finalPos = _initialLeapPosition + new Vector3(-dir.x * _leapAttackRange, _hitsCache[0].transform.position.y, -dir.z * _leapAttackRange);
                    if (_debugLogsEnemyPatrol) Debug.Log("Too Far from Leap Attack Range, jump closest direct point");
                }
                //Debug.Log(finalPos);
                return finalPos;
            }
        }

        //when is affected by light during the leap
        private IEnumerator FallCoroutine()
        {
            float count = 0;
            WaitForFixedUpdate delay = new WaitForFixedUpdate();
            Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, 20, LayerMask.NameToLayer("Terrain"));
            Vector3 finalPos = new Vector3(transform.position.x, hit.point.y + .16f, transform.position.z);
            while (count < 1)
            {
                count += Time.fixedDeltaTime / _fallDuration;
                transform.position = Vector3.Lerp(transform.position, finalPos, count);
                yield return delay;
            }
            _navMeshAgent.enabled = true;
            _navMeshAgent.isStopped = false;
            _fallCoroutine = null;
        }

        private void SetToChaseSpeed()
        {
            _navMeshAgent.speed = _currentMovementData ? _currentMovementData.ChaseSpeed : _chaseSpeed;
        }

        private void SetToWalkSpeed()
        {
            _navMeshAgent.speed = _currentMovementData ? _currentMovementData.WalkSpeed : _baseSpeed;
        }

        public void SetWalkArea(EnemyWalkArea area)
        {
            _currentWalkArea = area;
        }

        public void SetMovementData(EnemyMovementData data)
        {
            _currentMovementData = data;
            _navMeshAgent.speed = _isChasing ? data.ChaseSpeed : data.WalkSpeed;
            _navMeshAgent.acceleration = data.Acceleration;
            _navMeshAgent.angularSpeed = data.RotationSpeed;
        }
        #endregion

        #region IluminatedMethods
        public void ChangeSpeed(float speed)
        {
            _navMeshAgent.speed = speed;
        }

        public void UpdateBehaviour(bool canWalkPath, bool canChaseTarget, bool isStopped, object lightType)
        {
            _canChaseTarget = canChaseTarget;
            _canWalkPath = canWalkPath;
            if (!canWalkPath && !canChaseTarget)
            {
                if (lightType is Ivayami.Player.Ability.Lantern)
                {
                    _chaseTargetPatience = _delayToLoseTarget;
                    _hitsCache[0] = PlayerMovement.Instance.GetComponent<CharacterController>();
                    _lastTargetPosition = _hitsCache[0].transform.position;
                }
                HandleAttackAnimationEnd();
            }
            UpdateMovement(isStopped);
        }

        public void ChangeTargetPoint(Vector3 targetPoint)
        {
            _navMeshAgent.SetDestination(targetPoint);
        }
        #endregion

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
            if (_drawPatrolPoints && _patrolType == PatrolTypes.BasicPatrol)
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
            if (_drawFogAttackDistance)
            {
                Gizmos.color = _fogAttackColor;
                Gizmos.DrawSphere(transform.position, _distanceToFogAttack);
            }
            if (_drawLeapAttackDistance)
            {
                Gizmos.color = _leapAttackColor;
                Gizmos.DrawSphere(transform.position, _distanceToLeapAttack);
            }
            if (_drawPredictPosition && Application.isPlaying && _hitsCache[0])
            {
                Vector3 currentPlayerMovement = new Vector3(PlayerMovement.Instance.MovementDirection.x, 0, PlayerMovement.Instance.MovementDirection.z);
                bool blocked = Physics.Raycast(new Ray(_hitsCache[0].transform.position + new Vector3(0, _navMeshAgent.height * .5f, 0), currentPlayerMovement.normalized), _predictDistance, _blockVisionLayer);
                Gizmos.color = blocked ? Color.red : _predictPositionColor;
                Gizmos.DrawLine(_hitsCache[0].transform.position + new Vector3(0, _navMeshAgent.height * .5f, 0),
                    _hitsCache[0].transform.position + new Vector3(0, _navMeshAgent.height * .5f, 0) + currentPlayerMovement.normalized * _predictDistance);
                Gizmos.DrawSphere(_hitsCache[0].transform.position + new Vector3(0, _navMeshAgent.height * .5f, 0) + currentPlayerMovement.normalized * _predictDistance, .1f);
                //Gizmos.DrawWireCube(_hitsCache[0].transform.position + new Vector3(0, _navMeshAgent.height * .5f, 0) + currentPlayerMovement.normalized * _predictDistance,
                //    new Vector3(_collision.radius * 1.5f, _navMeshAgent.height, _collision.radius * 1.5f));
            }
            if (_drawLeapAttackRange)
            {
                Gizmos.color = _leapAttackRangeColor;
                Gizmos.DrawSphere(transform.position, _leapAttackRange);
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
            if (_distanceToFogAttack < _collision.radius + .2f) _distanceToFogAttack = _collision.radius + .2f;
            //_navMeshAgent.stoppingDistance = _distanceToFogAttack;
        }
#endif
        #endregion
    }
}