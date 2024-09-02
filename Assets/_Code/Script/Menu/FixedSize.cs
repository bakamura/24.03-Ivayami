using UnityEngine;

namespace Ivayami.UI {
    public class FixedSize : MonoBehaviour {

        [Header("Parameters")]

        [SerializeField] private float _sizeY;

        private void Start() {
            RectTransform rectTransform = GetComponent<RectTransform>();
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta[1] = _sizeY;
            rectTransform.sizeDelta = sizeDelta;
        }

    }
}
