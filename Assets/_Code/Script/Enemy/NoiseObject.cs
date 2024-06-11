using UnityEngine;

namespace Ivayami.Enemy
{
    public class NoiseObject : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _noiseRange;
        [SerializeField, Min(0f), Tooltip("Multiplicative")] private float _speedIncrease= 1;
        [SerializeField, Min(0f)] private float _durationInPlace;
        [SerializeField] private LayerMask _noiseTargetLayer;

#if UNITY_EDITOR
        [SerializeField] private Color _gizmoColor = Color.red;
#endif

        private Collider[] _hitsCache = new Collider[8];
        private void OnTriggerEnter(Collider other)
        {
            Physics.OverlapSphereNonAlloc(transform.position, _noiseRange, _hitsCache, _noiseTargetLayer);
            for (int i = 0; i < _hitsCache.Length; i++)
            {
                if (_hitsCache[i] && _hitsCache[i].TryGetComponent<IChangeTargetPoint>(out IChangeTargetPoint temp))
                {
                    temp.GoToPoint(transform, _speedIncrease, _durationInPlace);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawSphere(transform.position, _noiseRange);
        }
#endif
    }
}