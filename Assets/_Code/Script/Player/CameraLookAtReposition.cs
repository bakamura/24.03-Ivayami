using UnityEngine;
//using Cinemachine;

namespace Ivayami.Player
{
    public class CameraLookAtReposition : MonoBehaviour
    {
        [SerializeField] private Transform _aimPoint;
        [SerializeField] private float _distanceFactor = .8f;
        [SerializeField] private LayerMask _obstaclesLayer;

        private float _maxDistance;
        private RaycastHit[] _hits = new RaycastHit[1];
        //private CinemachineFreeLook _camera;
        private int _hitsCount;

        private void Awake()
        {
            _maxDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(_aimPoint.position.x, 0, _aimPoint.position.z));
            //_camera = FindObjectOfType<CinemachineFreeLook>();
        }

        private void Update()
        {
            Vector3 origin = new Vector3(transform.position.x, _aimPoint.position.y, transform.position.z);
            _hitsCount = Physics.RaycastNonAlloc(origin,
                _aimPoint.right,
                _hits, _maxDistance, _obstaclesLayer);
            if (_hitsCount > 0)
            {
                //Debug.Log($"HIT {_hits[0].collider.name}");
                //_camera.LookAt = _aimPoint.parent;
                float distance = Vector3.Distance(origin, _hits[0].point);
                _aimPoint.localPosition = new Vector3(distance * _distanceFactor, _aimPoint.localPosition.y, _aimPoint.localPosition.z);
            }
            else
            {
                //Debug.Log("NOTHING");
                //_camera.LookAt = _aimPoint;
                _aimPoint.localPosition = new Vector3(_maxDistance, _aimPoint.localPosition.y, _aimPoint.localPosition.z);
            }
        }

        //private void OnDrawGizmosSelected()
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawSphere(_aimPoint.transform.position, .01f);
        //}
    }
}