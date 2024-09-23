using UnityEngine;
using Ivayami.Player;
using UnityEngine.Events;
using UnityEngine.AI;

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
        private NavMeshAgent _currentAgent;

        [ContextMenu("TP")]
        public void Teleport()
        {
            if(_teleportType == TeleportTypes.Player)
            {
                PlayerMovement.Instance.SetPosition(transform.position);
                PlayerMovement.Instance.SetTargetAngle(transform.rotation.eulerAngles.y);
                PlayerCamera.Instance.FreeLookCam.PreviousStateIsValid = false;
                PlayerCamera.Instance.FreeLookCam.m_YAxis.Value = .5f;
                PlayerCamera.Instance.FreeLookCam.m_XAxis.Value = transform.rotation.eulerAngles.y;//Vector3.SignedAngle(PlayerCamera.Instance.MainCamera.transform.forward, transform.forward, Vector3.up);
            }
            else
            {
                if(_teleportTarget.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.position = transform.position;
                    rb.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                } 
                else if(_teleportTarget.TryGetComponent<NavMeshAgent>(out _currentAgent))
                {
                    _currentAgent.enabled = false;
                    _teleportTarget.SetPositionAndRotation(transform.position, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
                }
                else _teleportTarget.SetPositionAndRotation(transform.position, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
            }
            if(_currentAgent) _currentAgent.enabled = true;
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