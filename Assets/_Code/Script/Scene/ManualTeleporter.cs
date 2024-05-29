using UnityEngine;
using Ivayami.Player;
using Cinemachine;

namespace Ivayami.Scene
{
    public sealed class ManualTeleporter : MonoBehaviour
    {
        [SerializeField] private TeleportTypes _teleportType;
        [SerializeField] private Transform _teleportTarget;
        public enum TeleportTypes
        {
            Player,
            Object
        }
#if UNITY_EDITOR
        [SerializeField] private Color _gizmoColor = Color.red;        
        [SerializeField, Min(0f)] private float _gizmoSize = .2f;       
#endif
        [ContextMenu("TP")]
        public void Teleport()
        {
            if(_teleportType == TeleportTypes.Player)
            {
                PlayerMovement.Instance.transform.position = transform.position;
                PlayerMovement.Instance.SetTargetAngle(transform.rotation.eulerAngles.y);
                CinemachineFreeLook temp = FindObjectOfType<CinemachineFreeLook>();

                temp.PreviousStateIsValid = false;
                temp.ForceCameraPosition(temp.LookAt.transform.position + -temp.LookAt.transform.forward * temp.m_Orbits[1].m_Radius, Quaternion.identity);
                temp.m_XAxis.Value = Vector3.SignedAngle(Camera.main.transform.forward, transform.forward, Vector3.up);        
            }
            else
            {
                _teleportTarget.SetLocalPositionAndRotation(transform.position, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
            }
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