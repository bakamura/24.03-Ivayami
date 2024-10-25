using UnityEngine;

namespace Ivayami.Puzzle
{
    public abstract class Activable : MonoBehaviour
    {
        [Header("PARAMETERS")]
        [SerializeField, Min(0), Tooltip("If 0 will start active")] protected int _activatosNeededToActivate;
        [SerializeField] private Activator[] _activators;
        [HideInInspector] public bool IsActive { get; protected set; }

        protected virtual void Awake()
        {
            for (int i = 0; i < _activators.Length; i++)
            {
                _activators[i].onActivate.AddListener(HandleOnActivate);
            }
        }

        protected virtual void HandleOnActivate()
        {
            byte currentActiveAmount = 0;
            for (int i = 0; i < _activators.Length; i++)
            {
                if (_activators[i].IsActive) currentActiveAmount++;
            }
            IsActive = currentActiveAmount >= _activatosNeededToActivate;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_activators == null) return;
            if (_activatosNeededToActivate > _activators.Length) _activatosNeededToActivate = _activators.Length;
        }
#endif
    }
}