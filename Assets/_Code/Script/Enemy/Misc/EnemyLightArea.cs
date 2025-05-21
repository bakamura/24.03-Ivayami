using UnityEngine;

namespace Ivayami.Enemy
{
    public class EnemyLightArea : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("if is 0 will only interact with FollowLight enemies")] private float _radius;
#if UNITY_EDITOR
        [SerializeField] private Color _debugColor = Color.yellow;
        [SerializeField] private bool _gizmoAlwaysOn;
#endif

        private void OnEnable()
        {
            if (!LightFocuses.Instance) return;
            if (_radius > 0)
                LightFocuses.Instance.LightAreaFocusUpdate(nameof(EnemyLightArea) + gameObject.name + GetInstanceID(), new LightFocuses.LightData(transform.position, _radius));
            else
                LightFocuses.Instance.LightPointFocusUpdate(nameof(EnemyLightArea) + gameObject.name + GetInstanceID(), new LightFocuses.LightData(transform.position));
        }

        private void OnDisable()
        {
            if (!LightFocuses.Instance) return;
            if(_radius > 0) LightFocuses.Instance.LightAreaFocusRemove(nameof(EnemyLightArea) + gameObject.name + GetInstanceID());
            else LightFocuses.Instance.LightPointFocusRemove(nameof(EnemyLightArea) + gameObject.name + GetInstanceID());
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_gizmoAlwaysOn)
            {
                DrawGizmos();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_gizmoAlwaysOn) DrawGizmos();
        }

        private void DrawGizmos()
        {
            Gizmos.color = _debugColor;
            Gizmos.DrawSphere(transform.position, _radius);
        }
#endif
    }
}