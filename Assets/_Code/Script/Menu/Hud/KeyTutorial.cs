using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Ivayami.UI;
using Ivayami.Puzzle;

public class KeyTutorial : MonoBehaviour {

    [Header("Parameters")]

    [SerializeField] private GameObject _uiPrefab;

    [Space(24)]

    [SerializeField] private InputActionReference _actionIndicator;
    [SerializeField] private Sprite _indicatorKeyboard;
    [SerializeField] private Sprite _indicatorGamepad;

    public void StartTutorial() {
        GameObject instance = Instantiate(_uiPrefab, FindObjectOfType<InfoUpdateIndicator>().GetComponentInChildren<Fade>().transform);
        InputCallbacks.Instance.AddEventToOnChangeControls((callbackContext) => KeyPressed(instance));
        instance.GetComponentInChildren<Image>().sprite = _indicatorKeyboard;
        _actionIndicator.action.performed += (callbackContext) => KeyPressed(instance);
    }

    private void KeyPressed(GameObject instance) {
        _actionIndicator.action.performed -= (callbackContext) => KeyPressed(instance);
        InputCallbacks.Instance.RemoveEventToOnChangeControls((callbackContext) => KeyPressed(instance));
        Destroy(instance);
    }

}
