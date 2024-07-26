using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Ivayami.Player;
using Ivayami.Audio;
using System;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider), typeof(EnemySounds))]
    public class PoliceOfficer : MonoBehaviour, IEnemyWalkArea, IChangeTargetPoint
    {
        [Header("Parameters")]
        [SerializeField, Min(0f)] private float _minDetectionRange;
        [SerializeField, Min(0f)] private float _detectionRange;
        [SerializeField, Range(0f, 180f)] private float _visionAngle = 60f;
        [SerializeField] private Vector3 _visionOffset;
        [SerializeField, Min(.02f)] private float _tickFrequency = .5f;
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

        public bool IsActive { get; private set; }
        public LayerMask TargetLayer => _targetLayer;

        private void Awake()
        {
            _collision = GetComponent<CapsuleCollider>();
            _behaviourTickDelay = new WaitForSeconds(_tickFrequency);
            _enemyAnimator = GetComponentInChildren<EnemyAnimator>();
            _enemySounds = GetComponent<EnemySounds>();
            _halfVisionAngle = _visionAngle / 2f;
            //_betweenPatrolPointsDelay = new WaitForSeconds(_delayBetweenPatrolPoints);
            //SetMovementData(_movementData);

            if (_navMeshAgent.stoppingDistance == 0) _navMeshAgent.stoppingDistance = _collision.radius + .2f;
        }

        private void Update()
        {
            _enemyAnimator.Walking(_navMeshAgent.velocity.sqrMagnitude > 0);
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
                PlayerStress.Instance.SetStressMin(0);
                //_navMeshAgent.isStopped = true;
                _navMeshAgent.velocity = Vector3.zero;
            }
        }

        private IEnumerator BehaviourCoroutine()
        {
            while (IsActive)
            {

                if (/*_canChaseTarget &&*/ CheckForTarget(_halfVisionAngle))
                {
                    if (!_isChasing && !_navMeshAgent.isStopped)
                    {
                        _navMeshAgent.isStopped = true;
                        _navMeshAgent.velocity = Vector3.zero;
                        _enemySounds.PlaySound(EnemySounds.SoundTypes.TargetDetected);
                        PlayerStress.Instance.SetStressMin(98);
                        _enemyAnimator.TargetDetected(HandleTargetDetected);
                    }
                    if(_isChasing) _navMeshAgent.SetDestination(_hitsCache[0].transform.position);
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
                                _isChasing = false;
                                if (_debugLog) Debug.Log("Last target point reached");
                            }
                        }
                        else _isChasing = false;
                    }
                    else
                    {
                        PlayerStress.Instance.SetStressMin(0);
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
                if (!stayInPath && CheckForTarget(_halfVisionAngle))
                {
                    targetDetected = true;
                    break;
                }
                _navMeshAgent.SetDestination(finalPos);
                yield return delay;
            }
            if (!targetDetected) yield return stayInPointDelay;
            _navMeshAgent.velocity = Vector3.zero;
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
            if (!_navMeshAgent.isStopped)
            {
                StopBehaviour();
                PlayerStress.Instance.SetStressMin(98);
                HandlePointReachedCoroutine(true, false, 0, target);
            }
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
            if (!_navMeshAgent.isStopped)
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
            _enemySounds.PlaySound(EnemySounds.SoundTypes.TakeDamage);
            _enemyAnimator.TakeDamage(OnAttackAnimationEnd);
        }

        private void HandleTargetDetected()
        {
            _navMeshAgent.speed = _currentMovementData.ChaseSpeed;
            _isChasing = true;
            _lastTargetPosition = _hitsCache[0].transform.position;
            _navMeshAgent.isStopped = false;
        }

        //private void HandleOnInteractAnimationEnd()
        //{
        //    StartBehaviour();
        //}
        #region Debug
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
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