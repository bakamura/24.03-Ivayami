using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Ivayami.Player;
using Ivayami.Audio;
using System;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider), typeof(EnemySounds))]
    public class PoliceOfficer : StressEntity, IEnemyWalkArea, IChangeTargetPoint
    {
        [Header("Parameters")]
        [SerializeField, Min(0f)] private float _minDetectionRange;
        [SerializeField, Min(0f)] private float _detectionRange;
        [SerializeField, Min(0.01f)] private float _delayToLoseTarget;
        [SerializeField, Min(0f)] private float _stayInLastTargetPositionDuration;
        [SerializeField, Range(0f, 180f)] private float _visionAngle = 60f;
        [SerializeField] private Vector3 _visionOffset;
        [SerializeField, Min(.02f)] private float _behaviourTickFrequency = .5f;
        [SerializeField, Min(0f)] private float _stressIncreaseOnAttackTarget;
        [SerializeField] private bool _startActive;
        [SerializeField] private bool _goToLastTargetPosition;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private LayerMask _blockVisionLayer;

        [Header("Debug")]
        [SerializeField] private bool _debugLog;
#if UNITY_EDITOR
        [SerializeField] private bool _drawMinDistance;
        [SerializeField] private Color _minDistanceAreaColor = Color.yellow;
        [SerializeField] private bool _drawDetectionRange;
        [SerializeField] private Color _detectionRangeAreaColor = Color.red;
        [SerializeField, Tooltip("if value in NavMeshAgent is 0, the final distance will be collider radius + 0.2")] private bool _drawStoppingDistance;
        [SerializeField, Tooltip("if value in NavMeshAgent is 0, the final distance will be collider radius + 0.2")] private Color _stoppingDistanceColor = Color.green;
        private Mesh _FOVMesh;
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
        private WaitForSeconds _behaviourTickDelay;
        private CapsuleCollider _collision;
        private bool _isChasing;
        private EnemyAnimator _enemyAnimator;
        private EnemySounds _enemySounds;
        private Vector3 _lastTargetPosition;
        private Collider[] _hitsCache = new Collider[1];
        private Coroutine _detectTargetPointOffBehaviourReachedCoroutine;
        private EnemyMovementData _currentMovementData;
        private EnemyWalkArea _currenWalkArea;
        private float _halfVisionAngle;
        private float _speedMultiplier;
        private float _chaseTargetPatience;
        private float _goToLastTargetPointPatience;

        public bool IsActive { get; private set; }
        public LayerMask TargetLayer => _targetLayer;

        #region MainBehaviour
        protected override void Awake()
        {
            base.Awake();
            _collision = GetComponent<CapsuleCollider>();
            _behaviourTickDelay = new WaitForSeconds(_behaviourTickFrequency);
            _enemyAnimator = GetComponentInChildren<EnemyAnimator>();
            _enemySounds = GetComponent<EnemySounds>();
            _halfVisionAngle = _visionAngle / 2f;
            //_betweenPatrolPointsDelay = new WaitForSeconds(_delayBetweenPatrolPoints);
            //SetMovementData(_movementData);

            if (_navMeshAgent.stoppingDistance == 0) _navMeshAgent.stoppingDistance = _collision.radius + .2f;
        }

        private void Start()
        {
            if (_startActive)
            {
                StartBehaviour();
            }
        }

        [ContextMenu("Start")]
        public void StartBehaviour()
        {
            if (!IsActive)
            {
                IsActive = true;
                //_navMeshAgent.isStopped = false;
                StartCoroutine(BehaviourCoroutine());
            }
        }
        [ContextMenu("Stop")]
        public void StopBehaviour()
        {
            if (IsActive)
            {
                StopCoroutine(BehaviourCoroutine());
                IsActive = false;
                _isChasing = false;
                //PlayerStress.Instance.SetStressMin(0);
                //_navMeshAgent.isStopped = true;
                _navMeshAgent.velocity = Vector3.zero;
                _enemyAnimator.Walking(0);
                isStressAreaActive = true;
            }
        }

        private IEnumerator BehaviourCoroutine()
        {
            while (IsActive)
            {
                _chaseTargetPatience = Mathf.Clamp(_chaseTargetPatience - _behaviourTickFrequency, 0, _delayToLoseTarget);
                if (CheckForTarget(_halfVisionAngle)) _chaseTargetPatience = _delayToLoseTarget;
                isStressAreaActive = _chaseTargetPatience <= 0;
                if (/*_canChaseTarget &&*/ _chaseTargetPatience > 0)
                {
                    if (!_isChasing && !_navMeshAgent.isStopped)
                    {
                        _navMeshAgent.isStopped = true;
                        _navMeshAgent.velocity = Vector3.zero;
                        _enemyAnimator.Walking(0);
                        _enemySounds.PlaySound(EnemySounds.SoundTypes.TargetDetected, true);
                        //PlayerStress.Instance.SetStressMin(98);
                        _enemyAnimator.TargetDetected(HandleTargetDetected);
                    }
                    if (_isChasing) _navMeshAgent.SetDestination(_hitsCache[0].transform.position);
                    if (_debugLog) Debug.Log("Chase Target");
                }
                else
                {
                    //lost target, will look in last seen point                    
                    if (_isChasing)
                    {
                        if (_goToLastTargetPosition)
                        {
                            _navMeshAgent.SetDestination(_lastTargetPosition);
                            if (_debugLog) Debug.Log($"Moving to last target position {_lastTargetPosition}");
                            if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
                            {
                                _goToLastTargetPointPatience += _behaviourTickFrequency;
                                if (_debugLog) Debug.Log("Last target point reached");
                                if (_goToLastTargetPointPatience >= _stayInLastTargetPositionDuration)
                                {
                                    _isChasing = false;
                                    _goToLastTargetPointPatience = 0;
                                }
                                else
                                {
                                    if(_debugLog) Debug.Log("Waiting in Last Target Point");
                                }
                            }
                        }
                        else _isChasing = false;
                    }
                    else
                    {
                        //PlayerStress.Instance.SetStressMin(0);
                        if (_currenWalkArea && _currenWalkArea.GetCurrentPoint(gameObject.GetInstanceID(), out EnemyWalkArea.Point point))
                        {
                            _navMeshAgent.speed = _currentMovementData.WalkSpeed;
                            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(point.Position.x, 0, point.Position.z)) <= _navMeshAgent.stoppingDistance)
                            {
                                yield return new WaitForSeconds(point.DelayToNextPoint);
                                _navMeshAgent.SetDestination(_currenWalkArea.GoToNextPoint(gameObject.GetInstanceID()).Position);
                            }
                            else
                            {
                                _navMeshAgent.SetDestination(point.Position);
                            }
                        }
                        if (_debugLog) Debug.Log("Patroling");
                    }
                    _enemyAnimator.Chasing(_isChasing);
                }
                _enemyAnimator.Walking(_navMeshAgent.velocity.magnitude);
                yield return _behaviourTickDelay;
            }
        }

        private bool CheckForTarget(float halfVisionAngle)
        {
            bool targetInsideRange = Physics.OverlapSphereNonAlloc(transform.position, _detectionRange, _hitsCache, _targetLayer) > 0;
            if (!targetInsideRange) return false;
            //if (targetInsideRange) _currentTargetColliderSizeFactor = _hitsCache[0].bounds.extents.z;
            bool blockingVision = Physics.Raycast(transform.position + _visionOffset, (_hitsCache[0].transform.position - transform.position).normalized, Vector3.Distance(transform.position, _hitsCache[0].transform.position), _blockVisionLayer);
            bool isInMinRange = Vector3.Distance(_hitsCache[0].transform.position, transform.position) <= _minDetectionRange;
            bool isInVisionAngle = Vector3.Angle(transform.forward, (_hitsCache[0].transform.position - transform.position).normalized) <= halfVisionAngle;

            if (_debugLog)
                Debug.Log($"target Inside Radius {targetInsideRange}, blocking vision {blockingVision}, is in Min range {isInMinRange}, is in Vision Angle {isInVisionAngle}");
            return targetInsideRange && !blockingVision && (isInMinRange || isInVisionAngle);
        }
        #endregion

        #region CustomBehaviours
        public void Attack()
        {
            if (!_navMeshAgent.isStopped)
            {
                if (_debugLog) Debug.Log("Attack Target");
                StopBehaviour();
                StopTargetPointReachedCoroutine();
                _navMeshAgent.isStopped = true;
                PlayerStress.Instance.AddStress(_stressIncreaseOnAttackTarget);
                _enemyAnimator.Attack(OnAttackAnimationEnd);
            }
        }

        private void OnAttackAnimationEnd()
        {
            _navMeshAgent.isStopped = false;
            StartBehaviour();
        }

        private IEnumerator DetectTargetPointOffBehaviourReachedCoroutine(Vector3 finalPos, bool stayInPath, bool autoStartBehaviour, float durationInPlace)
        {
            WaitForFixedUpdate delay = new WaitForFixedUpdate();
            WaitForSeconds stayInPointDelay = new WaitForSeconds(durationInPlace);
            bool targetDetected = false;
            _navMeshAgent.SetDestination(finalPos);
            while (Vector3.Distance(new Vector3(transform.position.x, _navMeshAgent.destination.y, transform.position.z), _navMeshAgent.destination) > _navMeshAgent.stoppingDistance)
            {
                _enemyAnimator.Walking(_navMeshAgent.velocity.magnitude);
                if (!stayInPath && CheckForTarget(_halfVisionAngle))
                {
                    targetDetected = true;
                    break;
                }
                _navMeshAgent.SetDestination(finalPos);
                yield return delay;
            }
            _navMeshAgent.velocity = Vector3.zero;
            _enemyAnimator.Walking(0);
            if (!targetDetected) yield return stayInPointDelay;
            if (_speedMultiplier > 0) ChangeSpeedMultiplier(0);
            _detectTargetPointOffBehaviourReachedCoroutine = null;
            if (autoStartBehaviour) StartBehaviour();
        }

        public void SetMovementData(EnemyMovementData data)
        {
            _currentMovementData = data;
            _navMeshAgent.speed = _isChasing ? data.ChaseSpeed : data.WalkSpeed;
            _navMeshAgent.acceleration = data.Acceleration;
            _navMeshAgent.angularSpeed = data.RotationSpeed;
        }

        public void SetWalkArea(EnemyWalkArea area)
        {
            _currenWalkArea = area;
        }

        public void GoToPointWithoutStop(Transform target)
        {
            //if (!_navMeshAgent.isStopped)
            //{
            _navMeshAgent.isStopped = false;
            StopBehaviour();
            //PlayerStress.Instance.SetStressMin(98);
            HandlePointReachedCoroutine(true, false, 0, target);
            //}
        }

        private void HandlePointReachedCoroutine(bool stayInPath, bool autoStartBehaviour, float durationInPlace, Transform target)
        {
            if (_detectTargetPointOffBehaviourReachedCoroutine == null)
            {
                _detectTargetPointOffBehaviourReachedCoroutine = StartCoroutine(DetectTargetPointOffBehaviourReachedCoroutine(target.position, stayInPath, autoStartBehaviour, durationInPlace));
            }
        }

        public void GoToPoint(Transform target)
        {
            GoToPoint(target, 1, 0);
        }

        public void GoToPoint(Transform target, float speedIncrease, float durationInPlace)
        {
            if (!_navMeshAgent.isStopped && IsActive)
            {
                StopBehaviour();
                ChangeSpeedMultiplier(speedIncrease);
                HandlePointReachedCoroutine(false, true, durationInPlace, target);
            }
        }

        private void ChangeSpeedMultiplier(float value)
        {
            if (value > 0)
            {
                _speedMultiplier = value;
                _navMeshAgent.speed *= _speedMultiplier;
            }
            else
            {
                _navMeshAgent.speed /= _speedMultiplier;
                _speedMultiplier = value;
            }
        }

        private void StopTargetPointReachedCoroutine()
        {
            if (_detectTargetPointOffBehaviourReachedCoroutine != null)
            {
                StopCoroutine(_detectTargetPointOffBehaviourReachedCoroutine);
                _detectTargetPointOffBehaviourReachedCoroutine = null;
            }
        }

        public void Trip()
        {
            if (_debugLog) Debug.Log("Trip Animation");
            StopBehaviour();
            StopTargetPointReachedCoroutine();
            _navMeshAgent.isStopped = true;
            _navMeshAgent.velocity = Vector3.zero;
            _enemySounds.PlaySound(EnemySounds.SoundTypes.TakeDamage, true);
            _enemyAnimator.TakeDamage(OnAttackAnimationEnd);
        }

        private void HandleTargetDetected()
        {
            _navMeshAgent.speed = _currentMovementData.ChaseSpeed;
            _isChasing = true;
            _lastTargetPosition = _hitsCache[0].transform.position;
            _navMeshAgent.isStopped = false;
        }
        #endregion
        //private void HandleOnInteractAnimationEnd()
        //{
        //    StartBehaviour();
        //}
        #region Debug
#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            if (_drawDetectionRange)
            {
                _FOVMesh = DebugUtilities.CreateConeMesh(transform, _visionAngle, _detectionRange);
                Gizmos.color = _detectionRangeAreaColor;
                Gizmos.DrawMesh(_FOVMesh, transform.position + _visionOffset, Quaternion.identity);
            }
            if (_drawMinDistance)
            {
                Gizmos.color = _minDistanceAreaColor;
                Gizmos.DrawSphere(transform.position, _minDetectionRange);
            }
            if (_drawStoppingDistance)
            {
                Gizmos.color = _stoppingDistanceColor;
                Gizmos.DrawLine(transform.position, transform.position + transform.right * _navMeshAgent.stoppingDistance);
            }
        }
#endif
        #endregion
    }
}