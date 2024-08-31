using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Player {
    public class PlayerFailEvent : MonoBehaviour {

        [SerializeField] private UnityEvent _onFailState;

        private void OnEnable() {
            PlayerStress.Instance.onFailState.AddListener(_onFailState.Invoke);
        }

        private void OnDisable() {
            PlayerStress.Instance.onFailState.RemoveListener(_onFailState.Invoke);
        }

    }
}
