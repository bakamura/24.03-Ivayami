using UnityEngine;
using TMPro;
using Ivayami.UI;

public class ReadableUI : MonoSingleton<ReadableUI> {

    public Menu Menu { get; private set; }

    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _content;

    protected override void Awake() {
        base.Awake();

        Menu = GetComponent<Menu>();
    }

    public void ShowReadable(string title, string content) {
        _title.text = title;
        _content.text = content;
        // Resize rect to fit text properly
        _content.rectTransform.anchoredPosition = Vector2.zero;
        Menu.Open();
    }

}
