using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Ivayami.UI;

public class KeyTutorial : MonoBehaviour {

    [Header("Parameters")]

    [SerializeField] private GameObject _uiPrefab;

    [Space(24)]

    private InputActionReference _actionIndicator;
    private Sprite _indicatorKeyboard;
    private Sprite _indicatorGamepad;

    public void StartTutorial() {
        GameObject instance = Instantiate(_uiPrefab, FindObjectOfType<InfoUpdateIndicator>().GetComponentInChildren<Fade>().transform);
        instance.GetComponentInChildren<Image>().sprite = _indicatorKeyboard;
        _actionIndicator.action.performed += (callbackContext) => KeyPressed(instance);
    }

    private void KeyPressed(GameObject instance) {
        Destroy(instance);
        _actionIndicator.action.performed -= (callbackContext) => KeyPressed(instance);
    }

}
