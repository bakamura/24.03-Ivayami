using UnityEngine;

namespace Ivayami.Puzzle
{
    public abstract class Activable : MonoBehaviour
    {
        [Header("PARAMETERS")]
        [SerializeField, Min(0), Tooltip("If 0 will start active")] protected int _activatorsNeededToActivate;
        [SerializeField] protected Activator[] activators;
        [HideInInspector] public bool IsActive { get; protected set; }
        protected byte currentActiveAmount;

        protected virtual void Awake()
        {
            for (int i = 0; i < activators.Length; i++)
            {
                activators[i].onActivate.AddListener(HandleOnActivate);
            }
        }

        protected virtual void HandleOnActivate()
        {
            currentActiveAmount = 0;
            for (int i = 0; i < activators.Length; i++)
            {
                if (activators[i].IsActive) currentActiveAmount++;
            }
            IsActive = currentActiveAmount >= _activatorsNeededToActivate;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (activators == null) return;
            if (_activatorsNeededToActivate > activators.Length) _activatorsNeededToActivate = activators.Length;
        }
#endif
    }
}