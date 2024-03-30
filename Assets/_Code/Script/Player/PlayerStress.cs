using UnityEngine;
using UnityEngine.Events;

namespace Paranapiacaba.Player {
    public class PlayerStress : MonoSingleton<PlayerStress> {

        public UnityEvent<float> onStressChange = new UnityEvent<float>();

        public void AddStress(float amount) {
            Debug.LogWarning("Method Not Implemented Yet");
        }

    }
}