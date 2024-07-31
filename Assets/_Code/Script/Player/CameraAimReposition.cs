using UnityEngine;
//using Cinemachine;

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
        private RaycastHit[] _hits = new RaycastHit[1];
        private int _hitsCount;

        protected override void Awake()
        {
            base.Awake();
            _defaultMaxDistance = _aimPoint.localPosition.x;//Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(_aimPoint.position.x, 0, _aimPoint.position.z));
            _currentMaxDistance = _defaultMaxDistance;
        }

        private void FixedUpdate()
        {
            Vector3 origin = new Vector3(transform.position.x, _aimPoint.position.y, _aimPoint.position.z);
            _hitsCount = Physics.RaycastNonAlloc(origin,
                _aimPoint.right,
                _hits, _currentMaxDistance, _obstaclesLayer);
            if (_hitsCount > 0)
            {
                //Debug.Log($"HIT {_hits[0].collider.name}");
                //_camera.LookAt = _aimPoint.parent;
                float distance = Vector3.Distance(origin, _hits[0].point);
                //Debug.Log(_hits[0].point);
                _aimPoint.localPosition = new Vector3(Mathf.MoveTowards(_aimPoint.localPosition.x, distance * _distanceFactor, Time.deltaTime / _lerpDuration)
                   , _aimPoint.localPosition.y, _aimPoint.localPosition.z);
                //_aimPoint.localPosition = new Vector3(distance * _distanceFactor, _aimPoint.localPosition.y, _aimPoint.localPosition.z);
            }
            else
            {               
                //Debug.Log("NOTHING");
                //_camera.LookAt = _aimP                oint;
                _aimPoint.localPosition = new Vector3(Mathf.MoveTowards(_aimPoint.localPosition.x, _currentMaxDistance, Time.deltaTime / _lerpDuration),
                    _aimPoint.localPosition.y, _aimPoint.localPosition.z);
                //_aimPoint.localPosition = new Vector3(_maxDistance, _aimPoint.localPosition.y, _aimPoint.localPosition.z);
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