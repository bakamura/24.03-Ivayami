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
        [SerializeField] private Color[] _stressColors;

        [Header("HeartBeat")]

        [SerializeField] private Image[] _beatImages;
        [SerializeField] private Sprite[] _beatSprites;
        [SerializeField, Min(0f)] private float _beatSpeed;
        [SerializeField, Min(0f)] private float[] _beatPixels;
        private int _beatCurrent;
        [SerializeField, Min(0f)] private float _beatPartitionPixels;

        [SerializeField] private float[] _stepStressMins;
        private Vector2 _initialPos;

        private void Start() {
            StressIndicatorSmoother.Instance.OnStressSmoothed.AddListener(StressUpdate);
            PlayerMovement.Instance.onStaminaUpdate.AddListener(StaminaSaturate);
            _initialPos = _beatImages[0].rectTransform.localPosition;
            StressUpdate(0);
            StaminaSaturate(1);
        }

        private void Update() {
            _beatImages[0].rectTransform.Translate(_beatSpeed * Time.deltaTime * Vector3.left, Space.Self);
            if (_beatImages[0].rectTransform.anchoredPosition.x < -_beatImages[0].rectTransform.rect.width) {
                _beatImages[0].rectTransform.SetLocalPositionAndRotation(_initialPos, Quaternion.identity);
                _beatImages[0].sprite = _beatImages[1].sprite;
                _beatImages[1].sprite = _beatSprites[_beatCurrent];
            }
        }

        private void StressUpdate(float value) {
            HeartBeatUpdate(value);
            StressColorize(value);
        }

        private void HeartBeatUpdate(float value) {
            for (int i = _stepStressMins.Length - 1; i >= 1; i--)
                if (value > _stepStressMins[i]) {
                    _beatCurrent = i;
                    return;
                }
            _beatCurrent = 0;
        }

        private void StressColorize(float value) {
            Color? newColor = value <= _stepStressMins[0] ? _stressColors[0] :
                             value >= _stepStressMins[_stepStressMins.Length - 1] ? _stressColors[_stressColors.Length - 1] :
                             null;
            if (!newColor.HasValue)
                for (int i = 0; i < _stepStressMins.Length; i++) {
                    if (value < _stepStressMins[i + 1]) {
                        newColor = Color.Lerp(_stressColors[i], _stressColors[i + 1], (value - _stepStressMins[i]) / (_stepStressMins[i + 1] - _stepStressMins[i]));
                        break;
                    }
                }
            foreach (Image coloredImage in _indicatorFills) coloredImage.color = newColor.Value;
        }

        private void StaminaSaturate(float value) {
            foreach (Image filledImage in _indicatorFills) filledImage.fillAmount = value;
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (_beatPixels?.Length != _stepStressMins?.Length) Array.Resize(ref _stepStressMins, _beatPixels.Length);
            if (_beatPixels?.Length != _stressColors?.Length) Array.Resize(ref _stressColors, _beatPixels.Length);
        }
#endif

    }
}
