using UnityEngine;

namespace Ivayami.Enemy
{
    public class EnemyLight : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField, Range(0f, 1f)] private float _debugColorAlpha = .5f;
        public float DebugColorAlpha => _debugColorAlpha;
#endif

        private void OnEnable()
        {
            if (LightFocuses.Instance) LightFocuses.Instance.FocusUpdate(nameof(EnemyLight) + gameObject.name + GetInstanceID(), transform.position);
        }

        private void OnDisable()
        {
            if (LightFocuses.Instance) LightFocuses.Instance.FocusRemove(nameof(EnemyLight) + gameObject.name + GetInstanceID());
        }
    }
}