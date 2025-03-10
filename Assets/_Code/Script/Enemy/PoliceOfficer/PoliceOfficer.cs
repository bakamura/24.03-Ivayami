using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Ivayami.Audio;
using System;
using Ivayami.Player;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider), typeof(EnemySounds))]
    public class PoliceOfficer : StressEntity, IEnemyWalkArea, IChangeTargetPoint
    {
        [Header("Officer Parameters")]
        [SerializeField, Min(.02f)] private float _behaviourTickFrequency = .5f;
        [SerializeField, Min(0f)] private float _minDetectionRange;
        [SerializeField, Min(0f)] private float _detectionRange;
        [SerializeField, Min(0.01f)] private float _delayToLoseTarget;
        [SerializeField, Range(0f, 180f)] private float _visionAngle = 60f;
        [SerializeField] private Vector3 _visionOffset;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private LayerMask _blockVisionLayer;
        //[SerializeField, Min(0f)] private float _stressIncreaseOnAttackTarget;
        [SerializeField] private bool _startActive;
        [SerializeField] private bool _goToLastTargetPosition;
        [SerializeField] private bool _loseTargetWhenHidden = true;
        [SerializeField, Min(0f)] private float _stayInLastTargetPositionDuration;
        [SerializeField] private HitboxInfo[] _attackAreaInfos;

        [Header("Officer Debug")]
        [SerializeField] private bool _debugLogPoliceOfficer;
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
        private EnemyAnimator _enemyAnimator;
        private EnemySounds _enemySounds;
        private EnemyMovementData _currentMovementData;
        private EnemyWalkArea _currenWalkArea;
        //private HitboxAttack _hitboxAttack;
        private CapsuleCollider _collision;
        private WaitForSeconds _behaviourTickDelay;
        private Collider[] _hitsCache = new Collider[1];
        private Coroutine _detectTargetPointOffBehaviourReachedCoroutine;
        private Coroutine _initializeCoroutine;
        private Coroutine _behaviourCoroutine;
        private Vector3 _lastTargetPosition;
        private bool _isChasing;
        private bool _directContactWithTarget;
        private float _halfVisionAngle;
        private float _chaseTargetPatience;
        private float _goToLastTargetPointPatience;
        private float _currentTargetColliderSizeFactor;

        public bool IsActive => _behaviourCoroutine != null;
        //public LayerMask TargetLayer => _targetLayer;

        public int ID => gameObject.GetInstanceID();
        public bool CanChangeWalkArea => true;

        #region MainBehaviour
        protected override void Awake()
        {
            base.Awake();
            _collision = GetComponent<CapsuleCollider>();
            _behaviourTickDelay = new WaitForSeconds(_behaviourTickFrequency);
            _enemyAnimator = GetComponentInChildren<EnemyAnimator>();
            _enemySounds = GetComponent<EnemySounds>();
            //_hitboxAttack = GetComponentInChildren<HitboxAttack>();
            _halfVisionAngle = _visionAngle / 2f;

            if (_navMeshAgent.stoppingDistance == 0) _navMeshAgent.stoppingDistance = _collision.radius + .2f;
        }

        private void OnEnable()
        {
            if (!_navMeshAgent.enabled && _initializeCoroutine == null) _initializeCoroutine = StartCoroutine(InitializeAgent());
            if (_startActive && !IsActive) StartBehaviour();
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
                if (!_navMeshAgent.enabled)
                {
                    _initializeCoroutine = StartCoroutine(InitializeAgent());
                    return;
                }
                _navMeshAgent.isStopped = false;
                UpdateBehaviourCoroutine(true);
            }
        }
        [ContextMenu("Stop")]
        public void StopBehaviour()
        {
            if (IsActive)
            {
                UpdateBehaviourCoroutine(false);
                StopTargetPointReachedCoroutine();
                _isChasing = false;
                _chaseTargetPatience = 0;
                _navMeshAgent.isStopped = true;
                _navMeshAgent.velocity = Vector3.zero;
                _enemyAnimator.Walking(0);
                isStressAreaActive = false;
            }
        }

        private void StopBehaviour(bool tryKeepChase)
        {
            if (IsActive)
            {
                UpdateBehaviourCoroutine(false);
                StopTargetPointReachedCoroutine();
                if (!tryKeepChase)
                {
                    _isChasing = false;
                    _chaseTargetPatience = 0;
                }
                _navMeshAgent.isStopped = true;
                _navMeshAgent.velocity = Vector3.zero;
                _enemyAnimator.Walking(0);
                isStressAreaActive = false;
            }
        }

        private void UpdateBehaviourCoroutine(bool isActive)
        {
            if (isActive && _behaviourCoroutine == null)
            {
                _behaviourCoroutine = StartCoroutine(BehaviourCoroutine());
            }
            else if (!isActive && _behaviourCoroutine != null)
            {
                StopCoroutine(_behaviourCoroutine);
                _behaviourCoroutine = null;
            }
        }

        private IEnumerator BehaviourCoroutine()
        {
            while (true)
            {
                _chaseTargetPatience = Mathf.Clamp(_chaseTargetPatience - _behaviourTickFrequency, 0, _delayToLoseTarget);
                if (CheckForTarget(_halfVisionAngle)) _chaseTargetPatience = _delayToLoseTarget;
                isStressAreaActive = _chaseTargetPatience <= 0;
                if (_chaseTargetPatience > 0)
                {
                    if (!_isChasing)
                    {
                        TargetDetected();
                    }
                    _navMeshAgent.speed = _currentMovementData.ChaseSpeed;
                    _navMeshAgent.SetDestination(_hitsCache[0].transform.position);
                    _lastTargetPosition = _hitsCache[0].transform.position;
                    if (_chaseTargetPatience == _delayToLoseTarget && Vector3.Distance(transform.position, _navMeshAgent.destination) <= _navMeshAgent.stoppingDistance + _currentTargetColliderSizeFactor)
                    {
                        Attack();
                    }
                    if (_debugLogPoliceOfficer) Debug.Log("Chase Target");
                }
                else
                {
                    //lost target, will look in last seen point                    
                    if (_isChasing)
                    {
                        _navMeshAgent.speed = _currentMovementData.ChaseSpeed;
                        GoToLastTargetPoint();
                    }
                    else
                    {
                        //Patrol
                        if (_currenWalkArea && _currenWalkArea.GetCurrentPoint(ID, out EnemyWalkArea.EnemyData point))
                        {
                            _navMeshAgent.speed = _currentMovementData.WalkSpeed;
                            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(point.Point.Position.x, 0, point.Point.Position.z)) <= _navMeshAgent.stoppingDistance)
                            {
                                _navMeshAgent.velocity = Vector3.zero;
                                _enemyAnimator.Walking(0);
                                yield return new WaitForSeconds(point.Point.DelayToNextPoint);
                                _navMeshAgent.SetDestination(_currenWalkArea.GoToNextPoint(ID).Point.Position);
                                if (_debugLogPoliceOfficer)
                                {
                                    _currenWalkArea.GetCurrentPoint(ID, out point);
                                    Debug.Log($"Change Patrol Point to {point.CurrentPointIndex}");
                                }
                            }
                            else
                            {
                                _navMeshAgent.SetDestination(point.Point.Position);
                            }
                        }
                        if (_debugLogPoliceOfficer) Debug.Log("Patroling");
                    }
                }
                _enemyAnimator.Chasing(_isChasing);
                _enemyAnimator.Walking(_navMeshAgent.velocity.magnitude);
                yield return _behaviourTickDelay;
            }
        }

        private bool CheckForTarget(float halfVisionAngle)
        {
            Vector3 rayOrigin = transform.position + _visionOffset;
            bool targetInsideRange = _detectionRange > 0 ? Physics.OverlapSphereNonAlloc(rayOrigin, _detectionRange, _hitsCache, _targetLayer, QueryTriggerInteraction.Ignore) > 0 : true;

            bool isInMinRange;
            Vector3 targetCenter = Vector3.zero;
            if (_hitsCache[0])
            {
                targetCenter = _hitsCache[0].transform.position + new Vector3(0, _hitsCache[0].bounds.size.y, 0);
                isInMinRange = Vector3.Distance(targetCenter, rayOrigin) <= _minDetectionRange;
            }
            else isInMinRange = Physics.OverlapSphereNonAlloc(rayOrigin, _minDetectionRange, _hitsCache, _targetLayer, QueryTriggerInteraction.Ignore) > 0;

            if (!_hitsCache[0]) return false;

            bool isHidden = (_loseTargetWhenHidden && PlayerMovement.Instance.hidingState != PlayerMovement.HidingState.None) || !_loseTargetWhenHidden;
            bool blockingVision = Physics.Raycast(rayOrigin, (targetCenter - rayOrigin).normalized, /*out RaycastHit hit,*/ Vector3.Distance(rayOrigin, targetCenter), _blockVisionLayer, QueryTriggerInteraction.Ignore);
            bool isInVisionAngle = Vector3.Angle(transform.forward, (targetCenter - rayOrigin).normalized) <= halfVisionAngle;
            _currentTargetColliderSizeFactor = _hitsCache[0].bounds.extents.z;

            if (_debugLogPoliceOfficer)
                Debug.Log($"is Hidden {isHidden}, blocking vision {blockingVision}, is in Min range {isInMinRange}, target Inside Radius {targetInsideRange}, is in Vision Angle {isInVisionAngle}");
            _directContactWithTarget = !isHidden && !blockingVision && (isInMinRange || (isInVisionAngle && targetInsideRange));
            return _directContactWithTarget;
        }
        #endregion

        #region CustomBehaviours
        public void Attack()
        {
            //if (!_navMeshAgent.isStopped)
            //{
            if (_debugLogPoliceOfficer) Debug.Log("Attack Target");
            StopBehaviour(true);
            _enemyAnimator.Attack(OnAttackAnimationEnd, OnAnimationStepChange);
            //}
        }

        private void TargetDetected()
        {
            StopBehaviour(true);
            _isChasing = true;
            _enemyAnimator.Walking(0);
            _enemySounds.PlaySound(EnemySounds.SoundTypes.TargetDetected);
            PlayerStress.Instance.AddStress(100, 79);
            _enemyAnimator.TargetDetected(StartBehaviour);
        }

        private void GoToLastTargetPoint()
        {
            if (_goToLastTargetPosition)
            {
                _navMeshAgent.SetDestination(_lastTargetPosition);
                if (_debugLogPoliceOfficer) Debug.Log($"Moving to last target position {_lastTargetPosition}");
                if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
                {
                    _goToLastTargetPointPatience += _behaviourTickFrequency;
                    if (_debugLogPoliceOfficer) Debug.Log("Last target point reached");
                    if (_goToLastTargetPointPatience >= _stayInLastTargetPositionDuration)
                    {
                        _isChasing = false;
                        _goToLastTargetPointPatience = 0;
                    }
                    else
                    {
                        if (_debugLogPoliceOfficer) Debug.Log("Waiting in Last Target Point");
                    }
                }
            }
            else _isChasing = false;
        }

        //private void HandleTargetDetected()
        //{
        //    _isChasing = true;
        //    StartBehaviour();
        //}

        public void OpenDoor()
        {
            if (_directContactWithTarget) return;
            StopBehaviour(true);
            _enemyAnimator.Interact(StartBehaviour);
        }

        private void OnAttackAnimationEnd()
        {
            //_hitboxAttack.UpdateHitbox(false, Vector3.zero, Vector3.zero, 0, 0);
            for (int i = 0; i < _attackAreaInfos.Length; i++)
            {
                _attackAreaInfos[i].Hitbox.UpdateHitbox(false, Vector3.zero, Vector3.zero, 0, 0);
            }
            if (PlayerStress.Instance && PlayerStress.Instance.FailState)
            {
                _enemyAnimator.Walking(0);
            }
            else
            {
                StartBehaviour();
            }
        }

        private void OnAnimationStepChange(float normalizedTime)
        {
            int currentAnimIndex = _enemyAnimator.GetCurrentAttackAnimIndex();
            for (int i = 0; i < _attackAreaInfos.Length; i++)
            {
                if (_attackAreaInfos[i].AnimationIndex == currentAnimIndex && normalizedTime >= _attackAreaInfos[i].MinInterval && normalizedTime <= _attackAreaInfos[i].MaxInterval)
                {
                    _attackAreaInfos[i].Hitbox.UpdateHitbox(true, _attackAreaInfos[i].Center, _attackAreaInfos[i].Size, _attackAreaInfos[i].StressIncreaseOnEnter, _attackAreaInfos[i].StressIncreaseOnStay);
                    return;
                }
            }
            //_hitboxAttack.UpdateHitbox(false, Vector3.zero, Vector3.zero, 0, 0);
        }

        private IEnumerator DetectTargetPointOffBehaviourReachedCoroutine(Vector3 finalPos, /*bool stayInPath,*/ bool autoStartBehaviour, float durationInPlace)
        {
            bool targetDetected = false;
            _navMeshAgent.SetDestination(finalPos);
            while (Vector3.Distance(new Vector3(transform.position.x, _navMeshAgent.destination.y, transform.position.z), _navMeshAgent.destination) > _navMeshAgent.stoppingDistance)
            {
                _enemyAnimator.Walking(_navMeshAgent.velocity.magnitude);
                if (/*!stayInPath &&*/ CheckForTarget(_halfVisionAngle))
                {
                    targetDetected = true;
                    break;
                }
                yield return _behaviourTickDelay;
            }
            if (!targetDetected)
            {
                _navMeshAgent.isStopped = true;
                _navMeshAgent.velocity = Vector3.zero;
                _enemyAnimator.Walking(0);
                float currentTimeInPlace = 0;
                while (currentTimeInPlace < durationInPlace && !targetDetected)
                {
                    if (CheckForTarget(_halfVisionAngle))
                    {
                        targetDetected = true;
                        break;
                    }
                    yield return _behaviourTickDelay;
                    currentTimeInPlace += _behaviourTickFrequency;
                }
            }
            _detectTargetPointOffBehaviourReachedCoroutine = null;
            if (targetDetected) TargetDetected();
            else if (autoStartBehaviour) StartBehaviour();
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

        private void HandlePointReachedCoroutine(/*bool stayInPath,*/ bool autoStartBehaviour, float durationInPlace, Transform target)
        {
            if (_detectTargetPointOffBehaviourReachedCoroutine == null)
            {
                _detectTargetPointOffBehaviourReachedCoroutine = StartCoroutine(DetectTargetPointOffBehaviourReachedCoroutine(target.position, autoStartBehaviour, durationInPlace));
                //_detectTargetPointOffBehaviourReachedCoroutine = StartCoroutine(DetectTargetPointOffBehaviourReachedCoroutine(target.position, stayInPath, autoStartBehaviour, durationInPlace));
            }
        }

        public void GoToPoint(Transform target, float speedIncrease, float durationInPlace)
        {
            if (!_isChasing)
            {
                StopBehaviour(true);
                _navMeshAgent.isStopped = false;
                _navMeshAgent.speed = speedIncrease;
                HandlePointReachedCoroutine(/*false,*/ true, durationInPlace, target);
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
        #endregion

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

        protected override void OnValidate()
        {
            base.OnValidate();
            if (_attackAreaInfos == null) return;
            for (int i = 0; i < _attackAreaInfos.Length; i++)
            {
                if (_attackAreaInfos[i].MinInterval > _attackAreaInfos[i].MaxInterval) _attackAreaInfos[i].MinInterval = _attackAreaInfos[i].MaxInterval;
            }
            //if (!_hitboxAttack)
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