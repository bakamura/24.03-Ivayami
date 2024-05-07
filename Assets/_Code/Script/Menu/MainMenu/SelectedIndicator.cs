using UnityEngine;

public class SelectedIndicator : MonoBehaviour{

    private RectTransform _rectTransform;

    private void Awake() {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Reposition(RectTransform rectTransform) {
        transform.parent = rectTransform.transform;
        _rectTransform.anchoredPosition = Vector2.zero;
        transform.SetAsFirstSibling();
    }

}
