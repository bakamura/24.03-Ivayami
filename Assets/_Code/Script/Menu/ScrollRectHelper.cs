using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectHelper : MonoBehaviour {

    private ScrollRect _scrollRect;

    private void Awake() {
        _scrollRect = GetComponent<ScrollRect>();
    }

    public void MoveContent(BaseEventData eventData) {
        Vector2 delta = (eventData as PointerEventData).delta;
        if (!_scrollRect.horizontal) delta[0] = 0;
        if (!_scrollRect.vertical) delta[1] = 0;
        _scrollRect.content.anchoredPosition += delta;
    }

}
