using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Puzzle
{
    public class PedestalGroup : Activable
    {
        [SerializeField] private UnityEvent _onActivate;
        private Pedestal[] _pedestals;

        protected override void Awake()
        {
            base.Awake();
            _pedestals = new Pedestal[activators.Length];
            for (int i = 0; i < activators.Length; i++)
            {
                _pedestals[i] = activators[i].GetComponent<Pedestal>();
            }
        }

        protected override void HandleOnActivate()
        {
            base.HandleOnActivate();
            if (IsActive)
            {
                for (int i = 0; i < _pedestals.Length; i++)
                {
                    if (!_pedestals[i].DeliverUI.CheckRequestsCompletion()) return;
                }
                _onActivate?.Invoke();
            }
        }
    }
}