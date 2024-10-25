using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Puzzle
{
    public class ActivableEvent : Activable
    {
        [SerializeField] private UnityEvent _onActivate;

        protected override void HandleOnActivate()
        {
            base.HandleOnActivate();
            if (IsActive) _onActivate?.Invoke();
        }
    }
}