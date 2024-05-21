using Ivayami.Enemy;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Ivayami.Player;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(Rigidbody))]
    public class WalkNPC : MonoBehaviour
    {
        [Header("Paramaters")]
        [SerializeField, Min(0f)] private float _maxSpeed;
        [SerializeField, Min(0f)] private float _aceleration;
        [SerializeField, Min(0f)] private float _rotationSpeed;
        [SerializeField, Min(0f)] private float _minDistanceFromPathPoint;
        [SerializeField] private Path[] _paths;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debugDraw;
        [SerializeField] private Color[] _gizmoColors;
        [SerializeField, Min(0f)] private float _gizmoSize;
#endif

        private byte _currentPathIndex;
        private WaitForSeconds _delay = new WaitForSeconds(_tick);
        private Rigidbody _rigidbody;
        private Vector3 _currentVelocity;
        private Vector3 _initialPosition;
        private EnemyAnimator _animator;
        private const float _tick = .05f;
        private Coroutine _walkCoroutine;
        private byte _currentPointIndex;

        [Serializable]
        private struct Path
        {
            public Vector3[] Points;
            public bool LookAtPlayer;
            public UnityEvent OnPathEnd;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<EnemyAnimator>();
            _initialPosition = transform.position;
            TryLookAtPlayer();
        }

        private void FixedUpdate()
        {
            _rigidbody.velocity = _currentVelocity;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_walkCoroutine == null)
            {
                _walkCoroutine = StartCoroutine(WalkCoroutine());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_walkCoroutine != null)
            {
                StopCoroutine(_walkCoroutine);
                _walkCoroutine = null;
                _animator.Walking(false);
                _currentVelocity = Vector3.zero;
            }
        }

        private IEnumerator WalkCoroutine()
        {
            Vector3 direction;
            Vector3 finalPosition;
            _animator.Walking(true);
            while (_currentPointIndex < _paths[_currentPathIndex].Points.Length)
            {
                finalPosition = _paths[_currentPathIndex].Points[_currentPointIndex] + _initialPosition;
                finalPosition = new Vector3(finalPosition.x, transform.position.y, finalPosition.z);
                direction = finalPosition - transform.position;
                direction = new Vector3(direction.x, 0, direction.z);
                _currentVelocity = Vector3.MoveTowards(_currentVelocity, direction.normalized * _maxSpeed, _aceleration * _tick);
                _currentVelocity = new Vector3(_currentVelocity.x, _rigidbody.velocity.y, _currentVelocity.z);

                transform.rotation = Quaternion.Euler(0, Mathf.MoveTowardsAngle(transform.rotation.eulerAngles.y, Quaternion.LookRotation(direction.normalized).eulerAngles.y, _rotationSpeed * _tick), 0);
                if (Vector3.Distance(transform.position, finalPosition) <= _minDistanceFromPathPoint)
                {
                    _currentPointIndex++;
                    _currentVelocity = Vector3.zero;
                    _rigidbody.velocity = _currentVelocity;
                }

                yield return _delay;
            }
            _currentVelocity = Vector3.zero;
            _animator.Walking(false);
            if (_currentPathIndex + 1 < _paths.Length)
            {
                _paths[_currentPathIndex].OnPathEnd?.Invoke();
                _currentPathIndex++;
                transform.position = _paths[_currentPathIndex].Points[0] + _initialPosition;
                _currentPointIndex = 0;
                TryLookAtPlayer();
            }
            else
            {
                gameObject.SetActive(false);
            }
            _walkCoroutine = null;
        }

        private void TryLookAtPlayer()
        {
            if (_paths[_currentPathIndex].LookAtPlayer)
            {
                Quaternion rot = Quaternion.LookRotation((PlayerMovement.Instance.transform.position - transform.position).normalized, Vector3.up);
                rot = new Quaternion(0, rot.y, 0, rot.w);
                transform.rotation = rot;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_debugDraw && _paths != null)
            {
                int a;
                Vector3 initialPos = _initialPosition != Vector3.zero ? _initialPosition : transform.position;
                for (int i = 0; i < _paths.Length; i++)
                {
                    Gizmos.color = _gizmoColors[i];
                    for (a = 0; a < _paths[i].Points.Length; a++)
                    {
                        Gizmos.DrawSphere(initialPos + _paths[i].Points[a], _gizmoSize);
                        Handles.Label(initialPos + _paths[i].Points[a] + new Vector3(0, _gizmoSize * 2, 0), a.ToString());
                    }
                    for (a = 0; a < _paths[i].Points.Length - 1; a++)
                    {
                        Gizmos.DrawLine(initialPos + _paths[i].Points[a], initialPos + _paths[i].Points[Mathf.Clamp(a + 1, 0, _paths[i].Points.Length - 1)]);
                    }
                }
            }
        }

        private void OnValidate()
        {
            if (_paths != null && _gizmoColors.Length != _paths.Length) Array.Resize(ref _gizmoColors, _paths.Length);
        }
#endif
    }
}