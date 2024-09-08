using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Player {
    public class PlayerFailEvent : MonoBehaviour {

        [SerializeField] private bool _uniqueEventOnFail;
        [SerializeField] private UnityEvent _onFail;
        [SerializeField] private bool _uniqueEventOnFailFade;
        [SerializeField] private bool _overrideFailLoad;
        [SerializeField] private UnityEvent _onFailFade;

        private void Awake() {
            if (_uniqueEventOnFail) _onFail.AddListener(UnsubscribeFail);
            if (_uniqueEventOnFailFade) _onFailFade.AddListener(UnsubscribeFailFade);
        }

        public void SubscribeFail() {
            if (_onFail.GetPersistentEventCount() > 0) PlayerStress.Instance.onFail.AddListener(_onFail.Invoke);
        }

        public void SubscribeFailFade() {
            if (_onFail.GetPersistentEventCount() > 0) {
                PlayerStress.Instance.onFailFade.AddListener(_onFailFade.Invoke);
                if (_overrideFailLoad) PlayerStress.Instance.OverrideFailLoad();
            }
        }

        public void UnsubscribeFail() {
            PlayerStress.Instance.onFail.RemoveListener(_onFail.Invoke);
        }

        public void UnsubscribeFailFade() {
            PlayerStress.Instance.onFailFade.RemoveListener(_onFailFade.Invoke);
        }

    }
}
