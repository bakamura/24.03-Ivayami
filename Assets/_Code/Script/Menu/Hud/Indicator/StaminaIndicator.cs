#if UNITY_EDITOR
using System;
#endif
using UnityEngine;
using UnityEngine.UI;
using Ivayami.Player;

namespace Ivayami.UI {
    public class StaminaIndicator : Indicator {

        [Header("Stamina Color")]

        [SerializeField] private Color _staminaOutColor;
        [SerializeField] private Image[] _staminaColoredImages;

        [Header("HeartBeat")]

        [SerializeField] private Image _beatImage;
        [SerializeField, Min(0f)] private float _beatSpeed;
        [SerializeField, Min(0f)] private float[] _beatPixels;
        private int _beatCurrent;
        [SerializeField, Min(0f)] private float _beatPartitionPixels;

        [SerializeField] private float[] _stepStressMins;

        private void Start() {
            PlayerMovement.Instance.onStaminaUpdate.AddListener(StaminaColorize);
            PlayerStress.Instance.onStressChange.AddListener(FillUpdate);
        }

        private void Update() {
            _beatImage.rectTransform.Translate(_beatSpeed * Time.deltaTime * Vector3.left);
            if (_beatImage.rectTransform.anchoredPosition.x < -_beatPartitionPixels * (_beatCurrent + 1)) _beatImage.rectTransform.Translate(_beatPartitionPixels * Vector3.right);
        }

        public override void FillUpdate(float value) {
            for (int i = _stepStressMins.Length - 1; i >= 0; i--)
                if (value > _stepStressMins[i]) {
                    _beatCurrent = i;
                    break;
                }
        }

        private void StaminaColorize(float value) {
            Color newColor = Color.Lerp(_staminaOutColor, Color.white, value);
            foreach (Image coloredImage in _staminaColoredImages) coloredImage.color = newColor;
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (_beatPixels?.Length != _stepStressMins?.Length) Array.Resize(ref _stepStressMins, _beatPixels.Length);
        }
#endif

    }
}
