using System.Collections;
using UnityEngine;

namespace Ivayami.Enemy
{
    public class NoiseObject : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _noiseRange;
        [SerializeField, Min(.02f)] private float _noiseRepeatInterval;
        [SerializeField, Min(0f)] private float _newSpeed= 1;
        [SerializeField, Min(0f)] private float _durationInPlace;
        [SerializeField] private LayerMask _noiseTargetLayer;
        [SerializeField] private LayerMask _blockNoiseLayer;

#if UNITY_EDITOR
        [SerializeField] private Color _gizmoColor = Color.red;
#endif
        private Collider[] _hitsCache = new Collider[8];
        private Coroutine _repeatNoiseCoroutine;

        private void OnTriggerEnter(Collider other)
        {
            GenerateNoise();
        }

        private void OnDisable()
        {
            StopGenerateNoise();
        }

        [ContextMenu("Noise")]
        public void GenerateNoise()
        {
            Physics.OverlapSphereNonAlloc(transform.position, _noiseRange, _hitsCache, _noiseTargetLayer);
            for (int i = 0; i < _hitsCache.Length; i++)
            {
                if (_hitsCache[i] && _hitsCache[i].TryGetComponent<IChangeTargetPoint>(out IChangeTargetPoint temp) &&
                    !Physics.Raycast(transform.position, (transform.position - temp.CurrentPosition).normalized, Vector3.Distance(temp.CurrentPosition, transform.position), _blockNoiseLayer))
                {
                    temp.GoToPoint(transform, _newSpeed, _durationInPlace);
                }
            }
        }

        public void GenerateNoiseRepeatedly()
        {
            if(_repeatNoiseCoroutine == null)
            {
               _repeatNoiseCoroutine = StartCoroutine(GenerateNoiseRepeatedlyCoroutine());
            }
        }

        public void StopGenerateNoise()
        {
            if(_repeatNoiseCoroutine != null)
            {
                StopCoroutine(_repeatNoiseCoroutine);
                _repeatNoiseCoroutine = null;
            }
        }

        private IEnumerator GenerateNoiseRepeatedlyCoroutine()
        {
            WaitForSeconds delay = new WaitForSeconds(_noiseRepeatInterval);
            while (true)
            {
                GenerateNoise();
                yield return delay;
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