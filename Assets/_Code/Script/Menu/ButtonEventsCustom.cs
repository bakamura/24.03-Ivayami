using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Ivayami.UI
{
    public class ButtonEventsCustom : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public UnityEvent<BaseEventData> OnSelectSelectable = new UnityEvent<BaseEventData>();
        public UnityEvent<PointerEventData> OnPointerEnterSelectable = new UnityEvent<PointerEventData>();
        public UnityEvent<PointerEventData> OnPointerExitSelectable = new UnityEvent<PointerEventData>();

        public void OnSelect(BaseEventData eventData)
        {
            if (KeepSelection.Instance.CanTriggerSelectEvent(gameObject)) OnSelectSelectable?.Invoke(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnterSelectable?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExitSelectable?.Invoke(eventData);
        }
    }
}