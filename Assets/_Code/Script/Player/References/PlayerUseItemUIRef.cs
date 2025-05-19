using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.UI
{
    public class PlayerUseItemUIRef : MonoBehaviour
    {
        [SerializeField] private bool _onItemActivationEventSubscribeOnStart;
        [SerializeField] private UnityEvent _onItemActivation;
        [SerializeField] private bool _onItemEffectEndEventSubscribeOnStart;
        [SerializeField] private UnityEvent _onItemEffectEnd;
        [SerializeField] private bool _onNotRequiredItemEventSubscribeOnStart;
        [SerializeField] private UnityEvent _onNotRequiredItem;
        [SerializeField] private bool _onItemAlreadyInEffectEventSubscribeOnStart;
        [SerializeField] private UnityEvent _onItemAlreadyInEffect;
        //[SerializeField] private bool _onNotEnoughStressToHealEventSubscribeOnStart;
        //[SerializeField] private UnityEvent _onNotEnoughStressToHeal;

        private bool _onItemActivationEventSubscribed;
        private bool _onItemEffectEndEventSubscribed;
        private bool _onNotRequiredItemEventSubscribed;
        private bool _onItemAlreadyInEffectEventSubscribed;
        //private bool _onNotEnoughStressToHealEventSubscribed;

        private void Start()
        {
            if (_onItemActivationEventSubscribeOnStart) SubscribeOnItemActivation();
            if (_onItemEffectEndEventSubscribeOnStart) SubscribeOnItemEffectEnd();
            if (_onNotRequiredItemEventSubscribeOnStart) SubscribeOnNotRequiredItem();
            if (_onItemAlreadyInEffectEventSubscribeOnStart) SubscribeOnItemAlreadyInEffect();
            //if (_onNotEnoughStressToHealEventSubscribeOnStart) SubscribeOnNotEnoughStressToHeal();
        }

        private void OnDisable()
        {
            if (_onItemActivationEventSubscribed) UnsubscribeOnItemActivation();
            if (_onItemEffectEndEventSubscribed) UnsubscribeOnItemEffectEnd();
            if (_onNotRequiredItemEventSubscribed) UnsubscribeOnNotRequiredItem();
            if (_onItemAlreadyInEffectEventSubscribed) UnsubscribeOnItemAlreadyInEffect();
            //if (_onNotEnoughStressToHealEventSubscribed) UnsubscribeOnNotEnoughStressToHeal();
        }


        #region Subscribe
        public void SubscribeOnItemActivation()
        {
            if (_onItemActivationEventSubscribed) return;
            PlayerUseItemUI.Instance.OnItemActivation.AddListener(_onItemActivation.Invoke);
            _onItemActivationEventSubscribed = true;
        }

        public void SubscribeOnItemEffectEnd()
        {
            if (_onItemEffectEndEventSubscribed) return;
            PlayerUseItemUI.Instance.OnItemEffectEnd.AddListener(_onItemEffectEnd.Invoke);
            _onItemEffectEndEventSubscribed = true;
        }

        public void SubscribeOnNotRequiredItem()
        {
            if (_onNotRequiredItemEventSubscribed) return;
            PlayerUseItemUI.Instance.OnNotRequiredConsumableItem.AddListener(_onNotRequiredItem.Invoke);
            _onNotRequiredItemEventSubscribed = true;
        }

        public void SubscribeOnItemAlreadyInEffect()
        {
            if (_onItemAlreadyInEffectEventSubscribed) return;
            PlayerUseItemUI.Instance.OnItemAlreadyInEffect.AddListener(_onItemAlreadyInEffect.Invoke);
            _onItemAlreadyInEffectEventSubscribed = true;
        }

        //public void SubscribeOnNotEnoughStressToHeal()
        //{
        //    if (_onNotRequiredItemEventSubscribed) return;
        //    PlayerUseItemUI.Instance.OnNotEnoughStressToHeal.AddListener(_onNotEnoughStressToHeal.Invoke);
        //    _onNotEnoughStressToHealEventSubscribed = true;
        //}
        #endregion

        #region Unsibscribe
        public void UnsubscribeOnItemActivation()
        {
            if (!_onItemActivationEventSubscribed) return;
            PlayerUseItemUI.Instance.OnItemActivation.RemoveListener(_onItemActivation.Invoke);
            _onItemActivationEventSubscribed = false;
        }

        public void UnsubscribeOnItemEffectEnd()
        {
            if (!_onItemEffectEndEventSubscribed) return;
            PlayerUseItemUI.Instance.OnItemEffectEnd.RemoveListener(_onItemEffectEnd.Invoke);
            _onItemEffectEndEventSubscribed = false;
        }

        public void UnsubscribeOnNotRequiredItem()
        {
            if (!_onNotRequiredItemEventSubscribed) return;
            PlayerUseItemUI.Instance.OnNotRequiredConsumableItem.RemoveListener(_onNotRequiredItem.Invoke);
            _onNotRequiredItemEventSubscribed = false;
        }

        public void UnsubscribeOnItemAlreadyInEffect()
        {
            if (!_onItemAlreadyInEffectEventSubscribed) return;
            PlayerUseItemUI.Instance.OnItemAlreadyInEffect.RemoveListener(_onItemAlreadyInEffect.Invoke);
            _onItemAlreadyInEffectEventSubscribed = false;
        }

        //public void UnsubscribeOnNotEnoughStressToHeal()
        //{
        //    if (!_onNotEnoughStressToHealEventSubscribed) return;
        //    PlayerUseItemUI.Instance.OnNotEnoughStressToHeal.RemoveListener(_onNotEnoughStressToHeal.Invoke);
        //    _onNotEnoughStressToHealEventSubscribed = false;
        //}
        #endregion
    }
}