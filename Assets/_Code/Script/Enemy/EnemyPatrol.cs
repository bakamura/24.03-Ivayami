using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Ivayami.Player;
using Ivayami.Audio;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider), typeof(EnemySounds))]
    public class EnemyPatrol : StressEntity
    {
        [Header("Enemy Parameters")]
        [SerializeField, Min(0f)] private float _minDetectionRange;
        [SerializeField, Min(0f)] private float _detectionRange;
        [SerializeField, Min(0.01f)] private float _delayToLoseTarget;
        [SerializeField, Range(0f, 180f)] private float _visionAngle = 60f;
        [SerializeField] private Vector3 _visionOffset;
        [SerializeField, Min(0f), Tooltip("the complete delay is _tickFrequency + this value")] private float _delayBetweenPatrolPoints;
        [SerializeField, Min(.02f)] private float _behaviourTickFrequency = .5f;
        [SerializeField, Min(0f)] private float _stressIncreaseOnTargetDetected;
        [SerializeField, Min(0f)] private float _stressIncreaseWhileChasing;
        [SerializeField, Min(0f)] private float _stressIncreaseOnAttackTarget;
        [SerializeField] private bool _startActive;
        [SerializeField] private bool _goToLastTargetPosition;
        [SerializeField] private bool _attackTarget;
        [SerializeField] private bool _loseTargetWhenHidden = true;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private LayerMask _blockVisionLayer;
        [SerializeField] private Vector3[] _patrolPoints;

        [Header("Debug")]
        [SerializeField] private bool _debugLog;
#if UNITY_EDITOR
        [SerializeField] private bool _drawMinDistance;
        [SerializeField] private Color _minDistanceAreaColor = Color.yellow;
        [SerializeField] private bool _drawDetectionRange;
        [SerializeField] private Color _detectionRangeAreaColor = Color.red;
        [SerializeField] private bool _drawPatrolPoints;
        [SerializeField] private Color _patrolPointsColor = Color.black;
        [SerializeField, Tooltip("if value in NavMeshAgent is 0, the final distance will be collider radius + 0.2")] private bool _drawStoppingDistance;
        [SerializeField, Tooltip("if value in NavMeshAgent is 0, the final distance will be collider radius + 0.2")] private Color _stoppingDistanceColor = Color.green;
        [SerializeField, Min(0)] private float _patrolPointRadius = .2f;
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
        private WaitForSeconds _betweenPatrolPointsDelay;
        private CapsuleCollider _collision;
        private Vector3 _initialPosition;
        private bool _isChasing;
        private EnemyAnimator _enemyAnimator;
        private EnemySounds _enemySounds;
        private Quaternion _initialRotation;
        private Vector3 _lastTargetPosition;
        private float _currentTargetColliderSizeFactor;
        private Collider[] _hitsCache = new Collider[1];
        private bool _canChaseTarget = true;
        private bool _canWalkPath = true;
        private float _chaseTargetPatience;
        private float _goToLastTargetPointPatience;

        public bool IsActive { get; private set; }
        public float CurrentSpeed => _navMeshAgent.speed;

        protected override void Awake()
        {
            base.Awake();
            _collision = GetComponent<CapsuleCollider>();
            _behaviourTickDelay = new WaitForSeconds(_behaviourTickFrequency);
            _betweenPatrolPointsDelay = new WaitForSeconds(_delayBetweenPatrolPoints);
            _enemyAnimator = GetComponentInChildren<EnemyAnimator>();
            _enemySounds = GetComponent<EnemySounds>();

            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            if (_navMeshAgent.stoppingDistance == 0) _navMeshAgent.stoppingDistance = _collision.radius + .2f;
        }

        private void Update()
        {
            if (_isChasing && _stressIncreaseWhileChasing > 0)
                PlayerStress.Instance.AddStress(_stressIncreaseWhileChasing * Time.deltaTime);
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
                _navMeshAgent.isStopped = false;
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
                StopMovement(true);
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
                            if (_debugLog) Debug.Log("Target Detected");
                            StopMovement(true);
                            _enemySounds.PlaySound(EnemySounds.SoundTypes.TargetDetected);
                            PlayerStress.Instance.AddStress(_stressIncreaseOnTargetDetected);
                            _enemyAnimator.TargetDetected(HandleTargetDetectedAnimationEnd);
                        }
                        _navMeshAgent.SetDestination(_hitsCache[0].transform.position);
                        _lastTargetPosition = _hitsCache[0].transform.position;
                        if (_debugLog) Debug.Log("Chase Target");
                        if (_attackTarget && Vector3.Distance(transform.position, _navMeshAgent.destination) <= _navMeshAgent.stoppingDistance + _currentTargetColliderSizeFactor)
                        {
                            StopMovement(true);
                            PlayerStress.Instance.AddStress(_stressIncreaseOnAttackTarget);
                            //_isChasing = false;
                            _enemyAnimator.Attack(HandleAttackAnimationEnd);
                            if (_debugLog) Debug.Log("Attack Target");
                        }
                    }
                    else
                    {
                        if (_isChasing)
                        {
                            if (_goToLastTargetPosition)
                            {                                
                                _navMeshAgent.SetDestination(_lastTargetPosition);
                                if (_debugLog) Debug.Log($"Moving to last target position {_lastTargetPosition}");
                                if (Vector3.Distance(transform.position, _lastTargetPosition) <= _navMeshAgent.stoppingDistance)
                                {
                                    _goToLastTargetPointPatience += _behaviourTickFrequency;
                                    StopMovement(false);
                                    if (_debugLog) Debug.Log("Last target point reached");
                                    if(_goToLastTargetPointPatience >= _delayBetweenPatrolPoints)
                                    {
                                        _isChasing = false;
                                        _goToLastTargetPointPatience = 0;
                                    }
                                }
                            }
                            else _isChasing = false;
                        }
                        else
                        {
                            if (_canWalkPath)
                            {
                                _navMeshAgent.SetDestination(_patrolPoints[currentPatrolPointIndex] + _initialPosition);
                                if (_debugLog) Debug.Log("Patroling");
                                if (Vector3.Distance(transform.position, _patrolPoints[currentPatrolPointIndex] + _initialPosition) <= _navMeshAgent.stoppingDistance)
                                {
                                    if (_patrolPoints.Length > 1)
                                    {
                                        StopMovement(false);
                                        yield return _betweenPatrolPointsDelay;
                                        currentPatrolPointIndex = (byte)(currentPatrolPointIndex + indexFactor);
                                        if (currentPatrolPointIndex == _patrolPoints.Length - 1) indexFactor = -1;
                                        else if (currentPatrolPointIndex == 0 && indexFactor == -1) indexFactor = 1;
                                        if (_debugLog) Debug.Log($"Change Patrol Point to {currentPatrolPointIndex}");
                                    }
                                    else
                                    {
                                        transform.rotation = Quaternion.RotateTowards(transform.rotation, _initialRotation, _navMeshAgent.angularSpeed * _behaviourTickFrequency);
                                    }
                                }
                            }
                        }
                    }
                    _enemyAnimator.Walking(_navMeshAgent.velocity.magnitude);
                }
                yield return _behaviourTickDelay;
            }
        }

        private void StopMovement(bool stopNavMeshAgent)
        {
            if(stopNavMeshAgent) _navMeshAgent.isStopped = true;
            _navMeshAgent.velocity = Vector3.zero;
            _enemyAnimator.Walking(0);
        }

        private bool CheckForTarget(float halfVisionAngle)
        {
            bool targetInsideRange = _detectionRange > 0 ? Physics.OverlapSphereNonAlloc(transform.position, _detectionRange, _hitsCache, _targetLayer) > 0 : true;

            bool isInMinRange;
            if (_hitsCache[0]) isInMinRange = Vector3.Distance(_hitsCache[0].transform.position, transform.position) <= _minDetectionRange;
            else isInMinRange = Physics.OverlapSphereNonAlloc(transform.position, _minDetectionRange, _hitsCache, _targetLayer) > 0;

            if (!_hitsCache[0]) return false;

            bool isHidden = (_loseTargetWhenHidden && PlayerMovement.Instance.hidingState != PlayerMovement.HidingState.None) || !_loseTargetWhenHidden;
            bool blockingVision = Physics.Raycast(transform.position + _visionOffset, (_hitsCache[0].transform.position - transform.position).normalized, Vector3.Distance(transform.position, _hitsCache[0].transform.position), _blockVisionLayer);
            bool isInVisionAngle = Vector3.Angle(transform.forward, (_hitsCache[0].transform.position - transform.position).normalized) <= halfVisionAngle;
            _currentTargetColliderSizeFactor = _hitsCache[0].bounds.extents.z;

            if (_debugLog)
                Debug.Log($"is Hidden {isHidden}, blocking vision {blockingVision}, is in Min range {isInMinRange}, target Inside Radius {targetInsideRange}, is in Vision Angle {isInVisionAngle}");
            return !isHidden && !blockingVision && (isInMinRange || (isInVisionAngle && targetInsideRange));
        }

        private void HandleAttackAnimationEnd()
        {
            _navMeshAgent.isStopped = false;
        }

        private void HandleTargetDetectedAnimationEnd()
        {
            _isChasing = true;
            _navMeshAgent.isStopped = false;
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
#endif
        #endregion
    }
}