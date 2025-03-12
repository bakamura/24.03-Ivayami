using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Ivayami.UI
{
    public class DirectionalSelectable : Selectable
    {
        [SerializeField] private UnityEvent _onUp;
        [SerializeField] private UnityEvent _onDown;
        [SerializeField] private UnityEvent _onLeft;
        [SerializeField] private UnityEvent _onRight;

        public override Selectable FindSelectableOnUp()
        {
            if (_onUp.GetPersistentEventCount() <= 0)
                return base.FindSelectableOnUp();

            OnUp();
            return this;
        }

        public override Selectable FindSelectableOnDown()
        {
            if (_onDown.GetPersistentEventCount() <= 0)
                return base.FindSelectableOnDown();

            OnDown();
            return this;
        }

        public override Selectable FindSelectableOnLeft()
        {
            if (_onLeft.GetPersistentEventCount() <= 0)
                return base.FindSelectableOnLeft();

            OnLeft();
            return this;
        }

        public override Selectable FindSelectableOnRight()
        {
            if (_onRight.GetPersistentEventCount() <= 0)
                return base.FindSelectableOnRight();

            OnRight();
            return this;
        }

        public void OnUp() => _onUp.Invoke();
        public void OnDown() => _onDown.Invoke();
        public void OnLeft() => _onLeft.Invoke();
        public void OnRight() => _onRight.Invoke();
    }


}