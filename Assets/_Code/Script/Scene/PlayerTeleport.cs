using UnityEngine;
using Ivayami.Player;
using Cinemachine;

namespace Ivayami.Scene
{
    public sealed class PlayerTeleport : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private Color _gizmoColor = Color.red;
        //[SerializeField, Min(0f)] private float _gizmoSize = .2f;
#endif
        [ContextMenu("TP")]
        public void TeleportPlayer()
        {
            PlayerMovement.Instance.transform.position = transform.position;
            PlayerMovement.Instance.SetTargetAngle(transform.rotation.eulerAngles.y);
            CinemachineFreeLook temp = FindObjectOfType<CinemachineFreeLook>();
            temp.ForceCameraPosition(temp.LookAt.transform.position + -temp.LookAt.transform.forward * temp.m_Orbits[1].m_Radius, Quaternion.identity);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireCube(transform.position + new Vector3(0, 0.875f, 0), new Vector3(.5f, 1.75f, .5f));
        }
#endif
    }
}