using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonEvents : MonoBehaviour, ISelectHandler, IPointerEnterHandler {

    [SerializeField] private UnityEvent<BaseEventData> _onSelect = new UnityEvent<BaseEventData>();
    [SerializeField] private UnityEvent<PointerEventData> _onPointerEnter = new UnityEvent<PointerEventData>();

    public void OnSelect(BaseEventData eventData) {
        _onSelect?.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        _onPointerEnter?.Invoke(eventData);
    }

}
