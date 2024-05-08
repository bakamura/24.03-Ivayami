using Ivayami.Enemy;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Ivayami.Player;
using System;

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
        [SerializeField] private bool _lookAtPlayer;
        [SerializeField] private Path[] _paths;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debugDraw;
        [SerializeField] private Color[] _gizmoColors;
        [SerializeField, Min(0f)] private float _gizmoSize;
#endif

        private bool _isWalking;
        private byte _currentPathIndex;
        private WaitForSeconds _delay = new WaitForSeconds(_tick);
        private Rigidbody _rigidbody;
        private Vector3 _currentVelocity;
        private Vector3 _initialPosition;
        private EnemyAnimator _animator;
        private const float _tick = .05f;

        [Serializable]
        private struct Path
        {
            public Vector3[] Points;
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
            if (!_isWalking)
            {
                _isWalking = true;
                _animator.Walking(_isWalking);
                StartCoroutine(WalkCoroutine());
            }
        }

        private IEnumerator WalkCoroutine()
        {
            byte count = 0;
            Vector3 initialPosition = transform.position;
            Quaternion finalRotation;
            Vector3 direction;
            while (count < _paths[_currentPathIndex].Points.Length)
            {
                direction = _initialPosition + _paths[_currentPathIndex].Points[count] - transform.position;
                direction = new Vector3(direction.x, 0, direction.z);
                finalRotation = Quaternion.LookRotation((_paths[_currentPathIndex].Points[count] + _initialPosition - initialPosition).normalized);
                finalRotation = new Quaternion(0, finalRotation.y, 0, finalRotation.w);

                _currentVelocity = Vector3.MoveTowards(_currentVelocity, direction.normalized * _maxSpeed, _aceleration * _tick);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, finalRotation, _rotationSpeed * _tick);
                if (Vector3.Distance(transform.position, _paths[_currentPathIndex].Points[count] + _initialPosition) <= _minDistanceFromPathPoint) count++;

                yield return _delay;
            }
            if (_currentPathIndex + 1 < _paths.Length)
            {
                _paths[_currentPathIndex].OnPathEnd?.Invoke();
                _currentPathIndex++;
                transform.position = _paths[_currentPathIndex].Points[0] + _initialPosition;
                TryLookAtPlayer();
            }
            else
            {
                gameObject.SetActive(false);
            }
            _isWalking = false;
            _currentVelocity = Vector3.zero;
            _animator.Walking(_isWalking);
        }

        private void TryLookAtPlayer()
        {
            if (_lookAtPlayer)
            {
                Quaternion rot = Quaternion.LookRotation((PlayerMovement.Instance.transform.position - transform.position).normalized, Vector3.up);
                rot = new Quaternion(0, rot.y, 0, rot.w);
                if (_lookAtPlayer) transform.rotation = rot;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_debugDraw && _paths != null)
            {
                for (int i = 0; i < _paths.Length; i++)
                {
                    Gizmos.color = _gizmoColors[i];
                    for (int a = 0; a < _paths[i].Points.Length; a++)
                    {
                        Vector3 initialPos = _initialPosition != Vector3.zero ? _initialPosition : transform.position;
                        Gizmos.DrawSphere(initialPos + _paths[i].Points[a], _gizmoSize);
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