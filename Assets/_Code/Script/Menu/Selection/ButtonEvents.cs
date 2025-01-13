using Ivayami.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonEvents : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private UnityEvent<BaseEventData> _onSelect = new UnityEvent<BaseEventData>();
    [SerializeField] private UnityEvent<PointerEventData> _onPointerEnter = new UnityEvent<PointerEventData>();
    [SerializeField] private UnityEvent<PointerEventData> _onPointerExit = new UnityEvent<PointerEventData>();

    public void OnSelect(BaseEventData eventData) {
        if (KeepSelection.Instance.CanTriggerSelectEvent(gameObject)) _onSelect?.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _onPointerEnter?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData) {
        _onPointerExit?.Invoke(eventData);
    }
}
