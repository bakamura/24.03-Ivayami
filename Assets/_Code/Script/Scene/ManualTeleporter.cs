using UnityEngine;
using Ivayami.Player;
using Cinemachine;
using System.Collections;
using UnityEngine.Events;

namespace Ivayami.Scene
{
    public sealed class ManualTeleporter : MonoBehaviour
    {
        [SerializeField] private TeleportTypes _teleportType;
        [SerializeField] private Transform _teleportTarget;
        [SerializeField] private UnityEvent _onTeleportEnd;
        public enum TeleportTypes
        {
            Player,
            Object
        }
#if UNITY_EDITOR
        [SerializeField] private Color _gizmoColor = Color.red;        
        [SerializeField, Min(0f)] private float _gizmoSize = .2f;
#endif

        private static CinemachineFreeLook _playerCamera;
        private Coroutine _repositionCameraCoroutine;
        [ContextMenu("TP")]
        public void Teleport()
        {
            if(_teleportType == TeleportTypes.Player)
            {
                if(!_playerCamera) _playerCamera = FindObjectOfType<CinemachineFreeLook>();
                PlayerMovement.Instance.SetPosition(transform.position);
                PlayerMovement.Instance.SetTargetAngle(transform.rotation.eulerAngles.y);
                
                if(_repositionCameraCoroutine != null)
                {
                    StopCoroutine(_repositionCameraCoroutine);
                    _repositionCameraCoroutine = null;
                }
                _repositionCameraCoroutine = StartCoroutine(RepositionPlayerCameraCoroutine());
            }
            else
            {
                if(_teleportTarget.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.position = transform.position;
                    rb.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                } 
                else _teleportTarget.SetPositionAndRotation(transform.position, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
                _onTeleportEnd?.Invoke();
            }
        }

        private IEnumerator RepositionPlayerCameraCoroutine()
        {
            yield return new WaitForFixedUpdate();
            _playerCamera.PreviousStateIsValid = false;
            _playerCamera.ForceCameraPosition(_playerCamera.LookAt.transform.position + -_playerCamera.LookAt.transform.forward * _playerCamera.m_Orbits[1].m_Radius, Quaternion.identity);
            _playerCamera.m_XAxis.Value = Vector3.SignedAngle(Camera.main.transform.forward, transform.forward, Vector3.up);
            _repositionCameraCoroutine = null;
            _onTeleportEnd?.Invoke();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _gizmoColor;
            if (_teleportType == TeleportTypes.Player) Gizmos.DrawWireCube(transform.position + new Vector3(0, 0.875f, 0), new Vector3(.5f, 1.75f, .5f));
            else Gizmos.DrawSphere(transform.position, _gizmoSize);
        }
#endif
    }
}