using UnityEngine;
using TMPro;
using Ivayami.UI;
using UnityEngine.UI;

namespace Ivayami.Puzzle {
    public class ReadableUI : MonoSingleton<ReadableUI> {

        public Menu Menu { get; private set; }

        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _content;

        [field: SerializeField] public Button CloseBtn { get; private set; }

        protected override void Awake() {
            base.Awake();

            Menu = GetComponent<Menu>();
        }

        public void ShowReadable(string title, string content) {
            _title.text = title;
            _content.text = content;
            float preferredHeight = LayoutUtility.GetPreferredHeight(_content.rectTransform);
            _content.rectTransform.sizeDelta = new Vector2(_content.rectTransform.sizeDelta.x, LayoutUtility.GetPreferredHeight(_content.rectTransform));
            _content.rectTransform.anchoredPosition = Vector2.zero;
            Menu.Open();
        }

    }
}
