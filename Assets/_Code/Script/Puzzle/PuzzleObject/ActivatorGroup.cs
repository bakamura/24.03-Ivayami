using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace Ivayami.Puzzle
{
    public class ActivatorGroup : Activable
    {
        [SerializeField] private UnityEvent _onActivate;
        [SerializeField] private bool _getChildActivators;

        protected override void Awake()
        {
            if (_getChildActivators)
            {
                Activator[] extraActivators = GetComponentsInChildren<Activator>(true);
                for(int i = 0; i < extraActivators.Length; i++)
                {
                    if (!activators.Contains(extraActivators[i])) activators.Append(extraActivators[i]);
                }
            }
            base.Awake();
        }

        protected override void HandleOnActivate()
        {
            base.HandleOnActivate();
            if (IsActive) _onActivate?.Invoke();
        }
    }
}