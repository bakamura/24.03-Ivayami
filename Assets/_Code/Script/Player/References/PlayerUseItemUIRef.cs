using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.UI
{
    public class PlayerUseItemUIRef : MonoBehaviour
    {
        [SerializeField] private bool _onHealActivationEventSubscribeOnStart;
        [SerializeField] private UnityEvent _onHealActivation;
        [SerializeField] private bool _onHealEndEventSubscribeOnStart;
        [SerializeField] private UnityEvent _onHealEnd;
        [SerializeField] private bool _onNotRequiredItemEventSubscribeOnStart;
        [SerializeField] private UnityEvent _onNotRequiredItem;
        [SerializeField] private bool _onAlreadyHealingEventSubscribeOnStart;
        [SerializeField] private UnityEvent _onAlreadyHealing;
        [SerializeField] private bool _onNotEnoughStressToHealEventSubscribeOnStart;
        [SerializeField] private UnityEvent _onNotEnoughStressToHeal;

        private bool _onHealActivationEventSubscribed;
        private bool _onHealEndEventSubscribed;
        private bool _onNotRequiredItemEventSubscribed;
        private bool _onAlreadyHealingEventSubscribed;
        private bool _onNotEnoughStressToHealEventSubscribed;

        private void Start()
        {
            if (_onHealActivationEventSubscribeOnStart) SubscribeOnHealActivation();
            if (_onHealEndEventSubscribeOnStart) SubscribeOnHealEnd();
            if (_onNotRequiredItemEventSubscribeOnStart) SubscribeOnNotRequiredItem();
            if (_onAlreadyHealingEventSubscribeOnStart) SubscribeOnAlreadyHealing();
            if (_onNotEnoughStressToHealEventSubscribeOnStart) SubscribeOnNotEnoughStressToHeal();
        }

        private void OnDisable()
        {
            if (_onHealActivationEventSubscribed) UnsubscribeOnHealActivation();
            if (_onHealEndEventSubscribed) UnsubscribeOnHealEnd();
            if (_onNotRequiredItemEventSubscribed) UnsubscribeOnNotRequiredItem();
            if (_onAlreadyHealingEventSubscribed) UnsubscribeOnAlreadyHealing();
            if (_onNotEnoughStressToHealEventSubscribed) UnsubscribeOnNotEnoughStressToHeal();
        }


        #region Subscribe
        public void SubscribeOnHealActivation()
        {
            PlayerUseItemUI.Instance.OnHealActivation.AddListener(_onHealActivation.Invoke);
            _onHealActivationEventSubscribed = true;
        }

        public void SubscribeOnHealEnd()
        {
            PlayerUseItemUI.Instance.OnHealEnd.AddListener(_onHealEnd.Invoke);
            _onHealEndEventSubscribed = true;
        }

        public void SubscribeOnNotRequiredItem()
        {
            PlayerUseItemUI.Instance.OnNotRequiredItem.AddListener(_onNotRequiredItem.Invoke);
            _onNotRequiredItemEventSubscribed = true;
        }

        public void SubscribeOnAlreadyHealing()
        {
            PlayerUseItemUI.Instance.OnAlreadyHealing.AddListener(_onAlreadyHealing.Invoke);
            _onAlreadyHealingEventSubscribed = true;
        }

        public void SubscribeOnNotEnoughStressToHeal()
        {
            PlayerUseItemUI.Instance.OnNotEnoughStressToHeal.AddListener(_onNotEnoughStressToHeal.Invoke);
            _onNotEnoughStressToHealEventSubscribed = true;
        }
        #endregion

        #region Unsibscribe
        public void UnsubscribeOnHealActivation()
        {
            PlayerUseItemUI.Instance.OnHealActivation.RemoveListener(_onHealActivation.Invoke);
            _onHealActivationEventSubscribed = false;
        }

        public void UnsubscribeOnHealEnd()
        {
            PlayerUseItemUI.Instance.OnHealEnd.RemoveListener(_onHealEnd.Invoke);
            _onHealEndEventSubscribed = false;
        }

        public void UnsubscribeOnNotRequiredItem()
        {
            PlayerUseItemUI.Instance.OnNotRequiredItem.RemoveListener(_onNotRequiredItem.Invoke);
            _onNotRequiredItemEventSubscribed = false;
        }

        public void UnsubscribeOnAlreadyHealing()
        {
            PlayerUseItemUI.Instance.OnAlreadyHealing.RemoveListener(_onAlreadyHealing.Invoke);
            _onAlreadyHealingEventSubscribed = false;
        }

        public void UnsubscribeOnNotEnoughStressToHeal()
        {
            PlayerUseItemUI.Instance.OnNotEnoughStressToHeal.RemoveListener(_onNotEnoughStressToHeal.Invoke);
            _onNotEnoughStressToHealEventSubscribed = false;
        }
        #endregion
    }
}