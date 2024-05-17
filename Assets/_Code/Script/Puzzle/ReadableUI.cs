using UnityEngine;
using TMPro;
using Ivayami.UI;

public class ReadableUI : MonoBehaviour {

    public Menu Menu { get; private set; }

    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _content;

    private void Awake() {
        Menu = GetComponent<Menu>();
    }

    public void ShowReadable(string title, string content) {
        _title.text = title;
        _content.text = content;
        _content.rectTransform.anchoredPosition = Vector2.zero;
        Menu.Open();
    }

}
