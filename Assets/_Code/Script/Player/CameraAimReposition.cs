using UnityEngine;

namespace Ivayami.Player
{
    public class CameraAimReposition : MonoSingleton<CameraAimReposition>
    {
        [SerializeField] private Transform _aimPoint;
        [SerializeField, Min(0f)] private float _distanceFactor = .8f;
        [SerializeField, Min(0f)] private float _lerpDuration = 1f;
        [SerializeField] private LayerMask _obstaclesLayer;
#if UNITY_EDITOR
        [SerializeField, Min(0f)] private float _gizmoSize;
#endif

        private float _defaultMaxDistance;
        private float _currentMaxDistance;
        private RaycastHit _hit;

        protected override void Awake()
        {
            base.Awake();
            _defaultMaxDistance = _aimPoint.localPosition.x;//Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(_aimPoint.position.x, 0, _aimPoint.position.z));
            _currentMaxDistance = _defaultMaxDistance;
        }

        private void FixedUpdate()
        {
            Vector3 origin = new Vector3(transform.position.x, _aimPoint.position.y, transform.position.z);
            if (Physics.Raycast(origin, _aimPoint.right, out _hit, _currentMaxDistance, _obstaclesLayer))
            {
                //Debug.Log($"HIT {_hits[0].collider.name}");
                _aimPoint.localPosition = new Vector3(Mathf.MoveTowards(_aimPoint.localPosition.x, Vector3.Distance(origin, _hit.point) * _distanceFactor, Time.deltaTime / _lerpDuration)
                   , _aimPoint.localPosition.y, _aimPoint.localPosition.z);
            }
            else
            {
                //Debug.Log("NOTHING");
                _aimPoint.localPosition = new Vector3(Mathf.MoveTowards(_aimPoint.localPosition.x, _currentMaxDistance, Time.deltaTime / _lerpDuration),
                    _aimPoint.localPosition.y, _aimPoint.localPosition.z);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance">if value < 0 will use default value</param>
        public void SetMaxDistance(float distance)
        {
            if (distance > 0) _currentMaxDistance = distance;
            else _currentMaxDistance = _defaultMaxDistance;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_aimPoint.transform.position, _gizmoSize);
        }
#endif
    }
}