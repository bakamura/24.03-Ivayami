using System.Collections;
using UnityEngine;
using System.Linq;
using System;
using Ivayami.Player;

namespace Ivayami.Enemy
{    
    public class StressEntity : MonoBehaviour
    {
        [Header("Stress Entity Parameters")]
        //[Space(10)]
        [SerializeField, Min(0f)] private float _stressIncreaseTickFrequency = .2f;
        [SerializeField, Tooltip("The smaller radius allways has the priority")] private StressAreaInfo[] _stressAreas;
        //[Header("StressArea Debug")]
        [SerializeField] private bool _debugLogsStressEntity;
#if UNITY_EDITOR
        [SerializeField] private bool _drawGizmos;
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
        private bool _isStressAreaActive = true;
        private StressAreaInfo[] _stressAreasInOrder;
        private Coroutine _stressIncreaseCoroutine;
        private WaitForSeconds _delay;
        private bool _targetInsideArea;
        private StressEntityCollision _stressCollision
        {
            get
            {
                if (!m_stressCollision) m_stressCollision = GetComponentInChildren<StressEntityCollision>();
                return m_stressCollision;
            }
        }
        private StressEntityCollision m_stressCollision;

        protected virtual void Awake()
        {
            _delay = new WaitForSeconds(_stressIncreaseTickFrequency);
            _stressAreasInOrder = _stressAreas.OrderBy(x => x.Range).ToArray();
        }

        public void EnterArea()
        {
            _targetInsideArea = true;
            if (isStressAreaActive && PlayerMovement.Instance) _stressIncreaseCoroutine ??= StartCoroutine(StressIncreaseCoroutine());
        }

        public void ExitArea()
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
                        if (_debugLogsStressEntity) Debug.Log($"Adding stress {_stressAreasInOrder[i].StressIncrease} with a max of {_stressAreasInOrder[i].MaxStress}");
                        PlayerStress.Instance.AddStress(_stressAreasInOrder[i].StressIncrease * _stressIncreaseTickFrequency, _stressAreasInOrder[i].MaxStress);
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
            if (_targetInsideArea && _isStressAreaActive)
            {
                EnterArea();
            }
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (_drawGizmos) DrawStressAreaGizmos(transform.position);
        }

        protected virtual void OnValidate()
        {
            if (_stressAreas != null && _stressAreas.Length > 0 && _stressCollision) _stressCollision.SphereCollider.radius = _stressAreas.Max(x => x.Range);
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