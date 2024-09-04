using System.Collections;
using UnityEngine;

namespace Ivayami.Puzzle
{
    public class MovableActivable : Activable
    {
        [Header("Parameters")]
        [SerializeField] private Vector3[] _travelPoints;
        [SerializeField, Min(0f)] private float _speed;
        [SerializeField, Min(0f)] private float _tick = .2f;
        [SerializeField, Min(0f)] private float _minDistanceFromTravelPoint = .2f;
        [SerializeField] private bool _lockMovementOnDeactivate;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _gizmoDraw;
        [SerializeField] private Color _gizmoColor;
        [SerializeField, Min(0f)] private float _gizmoSize;
#endif

        private sbyte _directionFactor = 1;
        private sbyte _currentPointIndex;
        private Vector3 _initialPosition;
        private Vector3 _finalPosition;
        private WaitForSeconds _delayTick;
        private Coroutine _moveCoroutine;
        private bool _isMoving;

        protected override void Awake()
        {
            base.Awake();
            _initialPosition = transform.position;
            _delayTick = new WaitForSeconds(_tick);
        }

        protected override void HandleOnActivate()
        {
            base.HandleOnActivate();
            if (IsActive)
            {
                _directionFactor = 1;
                if (_moveCoroutine == null)
                {
                    _isMoving = true;
                    _moveCoroutine = StartCoroutine(MoveCoroutine());
                }
            }
            else
            {
                _directionFactor = -1;
                if (_lockMovementOnDeactivate)
                {
                    if(_moveCoroutine != null)
                    {
                        StopCoroutine(_moveCoroutine);
                        _moveCoroutine = null;
                        _isMoving = false;
                    }
                }
            }
            UpdateTravelPoint();
        }

        private IEnumerator MoveCoroutine()
        {
            while (_isMoving)
            {
                if(Vector3.Distance(transform.position, _finalPosition) > _minDistanceFromTravelPoint) 
                    transform.position += _speed * _tick * (_finalPosition - transform.position).normalized;
                else
                    UpdateTravelPoint();
                yield return _delayTick;
            }
            _moveCoroutine = null;
        }

        private void UpdateTravelPoint()
        {
            _currentPointIndex += _directionFactor;
            if (_currentPointIndex >= _travelPoints.Length)
            {
                _currentPointIndex = (sbyte)(_travelPoints.Length - 1);
                _isMoving = false;
            }
            else if (_currentPointIndex < 0)
            {
                _currentPointIndex = 0;
                _isMoving = false;
            }
            _finalPosition = _initialPosition + _travelPoints[_currentPointIndex];
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_gizmoDraw)
            {
                Vector3 initialPos = _initialPosition != Vector3.zero ? _initialPosition : transform.position;
                Gizmos.color = _gizmoColor;
                int i;
                for (i = 0; i < _travelPoints.Length; i++)
                {
                    Gizmos.DrawSphere(initialPos + _travelPoints[i], _gizmoSize);
                    UnityEditor.Handles.Label(initialPos + _travelPoints[i] + new Vector3(0, _gizmoSize * 2, 0), i.ToString());
                }
                for (i = 0; i < _travelPoints.Length - 1; i++)
                {
                    Gizmos.DrawLine(initialPos + _travelPoints[i], initialPos + _travelPoints[Mathf.Clamp(i + 1, 0, _travelPoints.Length - 1)]);
                }
            }
            
        }
#endif
    }
}