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

        [SerializeField] private Image _beatImage;
        [SerializeField, Min(0f)] private float _beatSpeed;
        [SerializeField, Min(0f)] private float[] _beatPixels;
        private int _beatCurrent;
        [SerializeField, Min(0f)] private float _beatPartitionPixels;

        [SerializeField] private float[] _stepStressMins;

        private void Start() {
            PlayerStress.Instance.onStressChange.AddListener(StressUpdate);
            PlayerMovement.Instance.onStaminaUpdate.AddListener(StaminaSaturate);
        }

        private void Update() {
            _beatImage.rectTransform.Translate(_beatSpeed * Time.deltaTime * Vector3.left);
            if (_beatImage.rectTransform.anchoredPosition.x < -_beatPartitionPixels * (_beatCurrent + 1)) _beatImage.rectTransform.Translate(_beatPartitionPixels * Vector3.right);
        }

        private void StressUpdate(float value) {
            HeartBeatUpdate(value);
            StressColorize(value);
        }

        private void HeartBeatUpdate(float value) {
            for (int i = _stepStressMins.Length - 1; i >= 0; i--)
                if (value > _stepStressMins[i]) {
                    _beatCurrent = i;
                    break;
                }
        } 

        private void StressColorize(float value) {
            Color newColor = Color.Lerp(_stressLowColor, _stressMaxColor, value);
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
