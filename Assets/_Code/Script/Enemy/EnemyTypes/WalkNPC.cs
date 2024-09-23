using System.Collections;
using UnityEngine;
using Ivayami.Player;
using UnityEngine.AI;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class WalkNPC : MonoBehaviour, IEnemyWalkArea
    {
        [Header("Paramaters")]
        [SerializeField] private bool _lookAtPlayerOnStart;
        [SerializeField] private EnemyWalkArea _fixedWalkArea;
        [SerializeField, Min(0f)] private float _minDistanceFromPathPoint;
        [SerializeField] private bool _scaleAnimationWithSpeed;
        [SerializeField] private EnemyWalkArea.PathCallback[] _pathsCallback;

        private WaitForSeconds _delay = new WaitForSeconds(_tick);
        private EnemyAnimator _animator;
        private const float _tick = .02f;
        private Coroutine _walkCoroutine;
        private NavMeshAgent _navMeshAgent;
        //private EnemyMovementData _currentMovementData;
        private EnemyWalkArea _currenWalkArea;

        public int ID => gameObject.GetInstanceID();

        public bool CanChangeWalkArea => _fixedWalkArea;

        private void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<EnemyAnimator>();
            if (_fixedWalkArea)
            {
                if (_fixedWalkArea.MovementData) SetMovementData(_fixedWalkArea.MovementData);
                SetWalkArea(_fixedWalkArea);
                _fixedWalkArea.AddEnemyToArea(this, gameObject.name);
            }
            if (PlayerMovement.Instance && _lookAtPlayerOnStart)
                TryLookAtPlayer();
        }

        private void OnTriggerEnter(Collider other)
        {
            StartBehaviour();
        }

        private void OnTriggerExit(Collider other)
        {
            StopBehaviour();
        }

        public void StartBehaviour()
        {
            if (_walkCoroutine == null)
            {
                _navMeshAgent.isStopped = false;
                _walkCoroutine = StartCoroutine(WalkCoroutine());
            }
        }

        public void StopBehaviour()
        {
            if (_walkCoroutine != null)
            {
                StopCoroutine(_walkCoroutine);
                _walkCoroutine = null;
                _animator.Walking(0);
                _navMeshAgent.isStopped = true;
                _navMeshAgent.velocity = Vector3.zero;
            }
        }

        private IEnumerator WalkCoroutine()
        {
            EnemyWalkArea.EnemyData currentPoint;
            while (_currenWalkArea && !_navMeshAgent.isStopped && _currenWalkArea.GetCurrentPoint(ID, out currentPoint))
            {
                _animator.Walking(_scaleAnimationWithSpeed ? _navMeshAgent.velocity.magnitude : 1);
                if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(currentPoint.Point.Position.x, 0, currentPoint.Point.Position.z)) <= _navMeshAgent.stoppingDistance)
                {
                    yield return new WaitForSeconds(currentPoint.Point.DelayToNextPoint);
                    _navMeshAgent.SetDestination(_currenWalkArea.GoToNextPoint(ID).Point.Position);
                    //Debug.Log($"Index {currentPoint.CurrentPointIndex}, going to next point");
                    for (int i = 0; i < _pathsCallback.Length; i++)
                    {
                        if (currentPoint.CurrentPointIndex == _pathsCallback[i].PointIndex)
                        {
                            _pathsCallback[i].OnPointReached?.Invoke();
                            break;
                        }
                    }
                }
                else
                {
                    //Debug.Log($"Index {currentPoint.CurrentPointIndex}, Position {currentPoint.Point.Position}");
                    _navMeshAgent.SetDestination(currentPoint.Point.Position);
                }
                yield return _delay;
            }
            _walkCoroutine = null;
        }

        public void TryLookAtPlayer()
        {
            Quaternion rot = Quaternion.LookRotation((PlayerMovement.Instance.transform.position - transform.position).normalized, Vector3.up);
            rot = new Quaternion(0, rot.y, 0, rot.w);
            transform.rotation = rot;
        }

        public void SetWalkArea(EnemyWalkArea area)
        {
            _currenWalkArea = area;
        }

        public void SetMovementData(EnemyMovementData data)
        {
            //_currentMovementData = data;
            _navMeshAgent.speed = data.WalkSpeed;
            _navMeshAgent.acceleration = data.Acceleration;
            _navMeshAgent.angularSpeed = data.RotationSpeed;
        }
    }
}