using UnityEngine;

namespace Ivayami.Enemy
{
    public class EnemyLight : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _radius;
        public float Radius => _radius;
#if UNITY_EDITOR
        [SerializeField] private Color _debugColor = Color.yellow;
        [SerializeField] private bool _gizmoAlwaysOn;
#endif

        private void OnEnable()
        {
            if (LightFocuses.Instance) LightFocuses.Instance.FocusUpdate(nameof(EnemyLight) + gameObject.name + GetInstanceID(), new LightFocuses.LighData(this, transform.position, _radius));
        }

        private void OnDisable()
        {
            if (LightFocuses.Instance) LightFocuses.Instance.FocusRemove(nameof(EnemyLight) + gameObject.name + GetInstanceID());
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