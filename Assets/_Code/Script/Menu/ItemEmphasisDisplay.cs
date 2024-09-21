using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ivayami.Player;
using Ivayami.UI;

public class ItemEmphasisDisplay : MonoSingleton<ItemEmphasisDisplay> {

    [Header("References")]

    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _description;
    private Menu _menu;

    private const string BLOCKER_ID = "ItemEmphasis";

    protected override void Awake() {
        base.Awake();

        _menu = GetComponent<Menu>();
    }

    public void DisplayItem(Sprite sprite, string name, string description) {
        _name.text = name;
        _image.sprite = sprite;
        _description.text = description;
        _menu.Open();
        PlayerMovement.Instance.ToggleMovement(BLOCKER_ID, false);
        PlayerActions.Instance.ChangeInputMap("Menu");

        ReturnAction.Instance.Set(CloseDisplay); // When UI has callbacks, set to when fade end
    }

    public void CloseDisplay() {
        _menu.Close();
        PlayerMovement.Instance.ToggleMovement(BLOCKER_ID, true);
        PlayerActions.Instance.ChangeInputMap("Player");


        ReturnAction.Instance.Set(null);
    }

}
