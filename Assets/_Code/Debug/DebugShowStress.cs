using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ivayami.Player;

public class DebugShowStress : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI _stressText;
    [SerializeField] private Image _stressBar;

    private void Start() {
        PlayerStress.Instance.onStressChange.AddListener((stress) => {
            _stressText.text = stress.ToString();
            _stressBar.fillAmount = stress / 100f;
        });
    }

}
