#if UNITY_EDITOR
using System;
#endif
using UnityEngine;
using UnityEngine.UI;
using Ivayami.Player;

namespace Ivayami.UI {
    public class StaminaIndicator : MonoBehaviour {


        [Header("Stress Color")]

        [SerializeField] private Image[] _indicatorFills;
        [SerializeField] private Color _stressMaxColor;
        [SerializeField] private Color _stressLowColor;

        [Header("HeartBeat")]

        [SerializeField] private Image[] _beatImages;
        [SerializeField]private Sprite[] _beatSprites;
        [SerializeField, Min(0f)] private float _beatSpeed;
        [SerializeField, Min(0f)] private float[] _beatPixels;
        private int _beatCurrent;
        [SerializeField, Min(0f)] private float _beatPartitionPixels;

        [SerializeField] private float[] _stepStressMins;

        private float _stressCapped;

        private void Start() {
            _stressCapped = PlayerStress.Instance.MaxStress - _stepStressMins[0];
            PlayerStress.Instance.onStressChange.AddListener(StressUpdate);
            PlayerMovement.Instance.onStaminaUpdate.AddListener(StaminaSaturate);

            StressUpdate(0);
            StaminaSaturate(1);
        }

        private void Update() {
            _beatImages[0].rectTransform.Translate(_beatSpeed * Time.deltaTime * Vector3.left);
            if (_beatImages[0].rectTransform.anchoredPosition.x < -_beatImages[0].rectTransform.rect.width) {
                _beatImages[0].rectTransform.Translate(_beatImages[0].rectTransform.rect.width * Vector3.right);
                _beatImages[0].sprite = _beatImages[1].sprite;
                _beatImages[1].sprite = _beatSprites[_beatCurrent];
            }
        }

        private void StressUpdate(float value) {
            HeartBeatUpdate(value);
            StressColorize(value);
        }

        private void HeartBeatUpdate(float value) {
            for (int i = _stepStressMins.Length - 1; i >= 0; i--)
                if (value > _stepStressMins[i]) {
                    _beatCurrent = i;
                    return;
                }
            _beatCurrent = 0;
        }

        private void StressColorize(float value) {
            Color newColor = Color.Lerp(_stressLowColor, _stressMaxColor, (value - _stepStressMins[0]) / _stressCapped);
            foreach (Image coloredImage in _indicatorFills) coloredImage.color = newColor;
        }

        private void StaminaSaturate(float value) {
            foreach (Image filledImage in _indicatorFills) filledImage.fillAmount = value;
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (_beatPixels?.Length != _stepStressMins?.Length) Array.Resize(ref _stepStressMins, _beatPixels.Length);
        }
#endif

    }
}
