using System.Collections;
using UnityEngine;
using System.Linq;
using System;
using Ivayami.Player;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(SphereCollider))]
    public class StressEntity : MonoBehaviour
    {
        [Header("StressArea Parameters")]
        [SerializeField, Min(0f)] private float _stressIncreaseTickFrequency = .2f;
        [SerializeField, Tooltip("The smaller radius allways has the priority")] private StressAreaInfo[] _stressAreas;
        [Header("Debug")]
#if UNITY_EDITOR
        [SerializeField] private bool _drawGizmos;
        private SphereCollider _sphereCollider;
#endif
        [Serializable]
        private struct StressAreaInfo
        {
            [Min(0f)] public float Range;
            [Min(0f)] public float MaxStress;
            [Min(0f)] public float StressIncrease;
#if UNITY_EDITOR
            public Color GizmoColor;
#endif
        }
        protected bool isStressAreaActive
        {
            get
            {
                return _isStressAreaActive;
            }
            set
            {
                SetActiveState(value);
            }
        }
        private bool _isStressAreaActive;
        private StressAreaInfo[] _stressAreasInOrder;
        private Coroutine _stressIncreaseCoroutine;
        private WaitForSeconds _delay;
        private bool _targetInsideArea;

        protected virtual void Awake()
        {
            _delay = new WaitForSeconds(_stressIncreaseTickFrequency);
            _stressAreasInOrder = _stressAreas.OrderBy(x => x.Range).ToArray();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            _targetInsideArea = true;
            if (isStressAreaActive) _stressIncreaseCoroutine ??= StartCoroutine(StressIncreaseCoroutine());
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (_stressIncreaseCoroutine != null)
            {
                StopCoroutine(_stressIncreaseCoroutine);
                _stressIncreaseCoroutine = null;
            }
            _targetInsideArea = false;
        }

        private IEnumerator StressIncreaseCoroutine()
        {
            while (_targetInsideArea && isStressAreaActive)
            {
                for (int i = 0; i < _stressAreasInOrder.Length; i++)
                {
                    if (Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position) <= _stressAreasInOrder[i].Range)
                    {
                        PlayerStress.Instance.AddStress(_stressAreasInOrder[i].StressIncrease, _stressAreasInOrder[i].MaxStress);
                        break;
                    }
                }
                yield return _delay;
            }
            _stressIncreaseCoroutine = null;
        }

        private void SetActiveState(bool isActive)
        {
            _isStressAreaActive = isActive;
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (_drawGizmos) DrawStressAreaGizmos(transform.position);
        }

        protected virtual void OnValidate()
        {
            if (!_sphereCollider) _sphereCollider = GetComponent<SphereCollider>();
            if (_stressAreas != null && _stressAreas.Length > 0) _sphereCollider.radius = _stressAreas.Max(x => x.Range);
        }

        private void DrawStressAreaGizmos(Vector3 startPosition)
        {
            if (_stressAreas != null)
            {
                for (int i = 0; i < _stressAreas.Length; i++)
                {
                    Gizmos.color = _stressAreas[i].GizmoColor;
                    Gizmos.DrawSphere(startPosition, _stressAreas[i].Range);
                }
            }
        }
#endif
    }
}