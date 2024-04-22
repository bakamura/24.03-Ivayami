using Paranapiacaba.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Paranapiacaba.UI {
    public class CurrentAbility : MonoBehaviour {

        [Header("Parameters")]

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _currentAbilityFrame;
        [SerializeField] private Image[] _abilityFrames;

        [Space(16)]

        [SerializeField] private float _showDuration;
        [SerializeField] private float _stayDuration;
        [SerializeField] private float _hideDuration;

        [Header("Resources")]

        [SerializeField] private string _abilityIconFolder;

        [Header("Cache")]

        private Coroutine _currentDisplayRoutine;
        private WaitForSeconds _stayWait;

        private void Awake() {
            _stayWait = new WaitForSeconds(_stayDuration);
        }

        private void Start() {
            PlayerActions.Instance.onAbilityChange.AddListener(DisplayCurrentAbility);
        }

        public void DisplayCurrentAbility(sbyte currentAbility) {
            if( _currentDisplayRoutine != null) StopCoroutine(_currentDisplayRoutine);
            Logger.Log(LogType.UI, $"Display Ability {currentAbility}");
            _currentDisplayRoutine = StartCoroutine(DisplayCurrentAbilityRoutine(currentAbility));
        }

        private IEnumerator DisplayCurrentAbilityRoutine(sbyte currentAbility) {
            _currentAbilityFrame.anchoredPosition = _abilityFrames[currentAbility].rectTransform.anchoredPosition;

            while (_canvasGroup.alpha < 1f) {
                _canvasGroup.alpha += Time.deltaTime / _showDuration;

                yield return null;
            }

            yield return _stayWait;

            while (_canvasGroup.alpha > 0f) {
                _canvasGroup.alpha -= Time.deltaTime / _hideDuration;

                yield return null;
            }

            _currentDisplayRoutine = null;
        }

        public void AddAbilityToDisplay(sbyte abilityId) {
            _abilityFrames[abilityId].sprite = Resources.Load<Sprite>($"{_abilityIconFolder}/AbilityIcon_{abilityId}");
            DisplayCurrentAbility(abilityId);
            Logger.Log(LogType.UI, $"Display Added Ability {abilityId}");
        }

    }
}