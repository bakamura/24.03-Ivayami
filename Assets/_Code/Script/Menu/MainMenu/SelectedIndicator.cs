using UnityEngine;

public class SelectedIndicator : MonoBehaviour{

    private RectTransform _rectTransform;

    [SerializeField] private Vector2 _anchoredPosOffset;

    private void Awake() {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Reposition(RectTransform rectTransform) {
        _rectTransform.SetParent(rectTransform.transform);
        _rectTransform.anchoredPosition = Vector2.zero;
        transform.SetAsFirstSibling();
    }

}
