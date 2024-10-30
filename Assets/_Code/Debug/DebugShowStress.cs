using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ivayami.Player;

public class DebugShowStress : MonoBehaviour {

#if UNITY_EDITOR
    [SerializeField] private TextMeshProUGUI _stressText;
    [SerializeField] private Image _stressBar;
    [SerializeField] private TextMeshProUGUI _staminaText;
    [SerializeField] private Image _staminaBar;

    private void Start() {
        if (!PlayerStress.Instance || !PlayerMovement.Instance) return;
        PlayerStress.Instance.onStressChange.AddListener((stress) => {
            _stressText.text = stress.ToString();
            _stressBar.fillAmount = stress / PlayerStress.Instance.MaxStress;
        });
        PlayerMovement.Instance.onStaminaUpdate.AddListener((stamina) =>
        {
            _staminaText.text = (stamina * PlayerMovement.Instance.MaxStamina).ToString();
            _staminaBar.fillAmount = stamina;
        });
    }
#endif
}
