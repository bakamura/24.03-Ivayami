using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Player {
    public class PlayerStressEvent : MonoBehaviour {

        [SerializeField] private bool _onFailEventSubscribeOnStart;
        [SerializeField] private bool _uniqueEventOnFail;
        [SerializeField] private UnityEvent _onFail;
        [SerializeField] private bool _onFailFadeEventSubscribeOnStart;
        [SerializeField] private bool _uniqueEventOnFailFade;
        [SerializeField] private bool _overrideFailLoad;
        [SerializeField] private UnityEvent _onFailFade;
        [SerializeField] private bool _onStressChangeEventSubscribeOnStart;
        [SerializeField] private StressEventData[] _onStressChangeEvents;

        [System.Serializable]
        private struct StressEventData
        {
            [Min(0f)] public float StressValue;
            public bool OnlyOnce;
            [System.NonSerialized] public bool EventTriggered;
            public UnityEvent Event;
        }
        private bool _onStressChangeEventSubscribed;
        private bool _onFailEventSubscribed;
        private bool _onFailFadeEventSubscribed;

        private void Start() {
            if (_onFailEventSubscribeOnStart) SubscribeFail();
            if (_onFailFadeEventSubscribeOnStart) SubscribeFailFade();
            if (_onStressChangeEventSubscribeOnStart) SubscribeOnStressChange();

            if (_uniqueEventOnFail) _onFail.AddListener(UnsubscribeFail);
            if (_uniqueEventOnFailFade) _onFailFade.AddListener(UnsubscribeFailFade);
        }

        private void OnDisable()
        {
            if (_onStressChangeEventSubscribed) UnsubscribeOnStressChange();            
            if (_onFailEventSubscribed)UnsubscribeFail();            
            if (_onFailFadeEventSubscribed) UnsubscribeFailFade();
        }

        public void SubscribeFail() {
            PlayerStress.Instance.onFail.AddListener(_onFail.Invoke);
            _onFailEventSubscribed = true;
        }

        public void SubscribeFailFade() {
            PlayerStress.Instance.onFailFade.AddListener(_onFailFade.Invoke);
            if (_overrideFailLoad) PlayerStress.Instance.OverrideFailLoad();
            _onFailFadeEventSubscribed = true;
        }

        public void SubscribeOnStressChange()
        {
            PlayerStress.Instance.onStressChange.AddListener(OnStressChange);
            _onStressChangeEventSubscribed = true;
        }

        public void UnsubscribeFail() {
            PlayerStress.Instance.onFail.RemoveListener(_onFail.Invoke);
            _onFailEventSubscribed = false;
        }

        public void UnsubscribeFailFade() {
            PlayerStress.Instance.onFailFade.RemoveListener(_onFailFade.Invoke);
            _onFailFadeEventSubscribed = false;
        }

        public void UnsubscribeOnStressChange()
        {
            PlayerStress.Instance.onStressChange.RemoveListener(OnStressChange);
            _onStressChangeEventSubscribed = false;
        }

        private void OnStressChange(float stressCurrent)
        {
            for(int i = 0; i < _onStressChangeEvents.Length; i++)
            {
                if (stressCurrent < _onStressChangeEvents[i].StressValue) continue;
                if (_onStressChangeEvents[i].OnlyOnce && _onStressChangeEvents[i].EventTriggered) continue;
                _onStressChangeEvents[i].EventTriggered = true;
                _onStressChangeEvents[i].Event?.Invoke();
            }
        }
    }
}
