using UnityEngine;
using UnityEngine.Events;

namespace Paranapiacaba.Player {
    public class PlayerStress : MonoSingleton<PlayerStress> {

        [Header("Events")]

        public UnityEvent<float> onStressChange = new UnityEvent<float>();
        public UnityEvent onFailState = new UnityEvent();

        [Header("Parameters")]

        [SerializeField] private float _stressMax;
        private float _stressCurrent;

        protected override void Awake() {
            base.Awake();

            onStressChange.AddListener(FailState);
        }

        public void AddStress(float amount) {
            _stressCurrent += amount;
            onStressChange.Invoke(_stressCurrent);
        }

        private void FailState(float stressCurrent) {
            if (stressCurrent >= _stressMax) onFailState.Invoke();
            // Prevent reinvoking
        }

    }
}