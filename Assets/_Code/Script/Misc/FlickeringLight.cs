using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ivayami.Misc
{
    [RequireComponent(typeof(Light), typeof(Collider))]
    public class FlickeringLight : MonoBehaviour
    {
        [Header("SETUP")]
        [SerializeField] private bool _autoStart;
        [SerializeField, Min(0.01f)] private float _tickFrequency;
        [SerializeField, Min(0)] private float _interpolationDuration;
        [Header("COLOR")]
        [SerializeField, Range(.01f, 100f)] private float _colorVariationChance;
        [SerializeField] private Color[] _colorVariation;
        [SerializeField] private bool _interpolateColor;
        [SerializeField] private AnimationCurve _interpolateColorCurve;
        [Header("RANGE")]
        [SerializeField, Range(.01f, 100f)] private float _rangeVariationChance;
        [SerializeField, Min(0f)] private float[] _rangeVariation;
        [SerializeField] private bool _interpolateRange;
        [SerializeField] private AnimationCurve _interpolateRangeCurve;
        [Header("INTENSITY")]
        [SerializeField, Range(.01f, 100f)] private float _intensityVariationChance;
        [SerializeField, Min(0f)] private float[] _intensityVariation;
        [SerializeField] private bool _interpolateIntensity;
        [SerializeField] private AnimationCurve _interpolateIntensityCurve;

        private Light _light
        {
            get
            {
                if (!m_light) m_light = GetComponent<Light>();
                return m_light;
            }
        }
        private Light m_light;
        private Coroutine _variationCoroutine;
        private Dictionary<string, Coroutine> _interpolationList = new Dictionary<string, Coroutine>();
        private bool _isActive;
        private const string _rangeKey = "range";
        private const string _intensityKey = "intensity";
        private const string _colorKey = "color";

        private void Awake()
        {
            if (_autoStart)
            {
                Activate();
                _light.enabled = false;
                //UpdateCoroutine(true);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            _light.enabled = true;
            UpdateCoroutine(true);
        }

        private void OnTriggerExit(Collider other)
        {
            _light.enabled = false;
            UpdateCoroutine(false);
        }

        public void Activate()
        {
            _isActive = true;
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        private void UpdateCoroutine(bool isActive)
        {
            if (!_isActive) return;
            if (isActive && _variationCoroutine == null)
            {
                _variationCoroutine = StartCoroutine(VariationCoroutine());
            }
            else if (!isActive && _variationCoroutine != null)
            {
                StopCoroutine(_variationCoroutine);
                _variationCoroutine = null;
            }
        }

        private IEnumerator VariationCoroutine()
        {
            WaitForSeconds delay = new WaitForSeconds(_tickFrequency);
            while (true)
            {
                float val = Random.Range(0f, 100f);

                if (_rangeVariation.Length > 0 && val <= _rangeVariationChance)
                {
                    if (_interpolateRange)
                    {
                        if (_interpolationList.ContainsKey(_rangeKey))
                        {
                            StopCoroutine(_interpolationList[_rangeKey]);
                            _interpolationList.Remove(_rangeKey);
                        }
                        _interpolationList.Add(_rangeKey, StartCoroutine(InterpolateCoroutine(0)));
                    }
                    else _light.range = _rangeVariation[Random.Range(0, _rangeVariation.Length)];
                    // Debug.Log($"Change Range {value}");
                }
                if (_intensityVariation.Length > 0 && val <= _intensityVariationChance)
                {
                    if (_interpolateIntensity)
                    {
                        if (_interpolationList.ContainsKey(_intensityKey))
                        {
                            StopCoroutine(_interpolationList[_intensityKey]);
                            _interpolationList.Remove(_intensityKey);
                        }
                        _interpolationList.Add(_intensityKey, StartCoroutine(InterpolateCoroutine(1)));
                    }
                    else _light.intensity = _intensityVariation[Random.Range(0, _intensityVariation.Length)];
                    //Debug.Log($"Change Intensity {_light.intensity}");
                }
                if (_colorVariation.Length > 0 && val <= _colorVariationChance)
                {
                    if (_interpolateColor && !_interpolationList.ContainsKey(_colorKey))
                    {
                        if (_interpolationList.ContainsKey(_colorKey))
                        {
                            StopCoroutine(_interpolationList[_colorKey]);
                            _interpolationList.Remove(_colorKey);
                        }
                        _interpolationList.Add(_colorKey, StartCoroutine(InterpolateCoroutine(2)));
                    }
                    else _light.color = _colorVariation[Random.Range(0, _colorVariation.Length)];
                    //Debug.Log($"Change Color {_light.color}");
                }

                yield return delay;
            }
        }

        //0 = range, 1 = intensity, 2 = color
        private IEnumerator InterpolateCoroutine(byte type)
        {
            WaitForFixedUpdate delay = new WaitForFixedUpdate();
            float count = 0;
            float finalValue = 0;
            string key = null;
            Color finalColor = Color.white;
            switch (type)
            {
                case 0:
                    key = _rangeKey;
                    finalValue = _rangeVariation[Random.Range(0, _rangeVariation.Length)];
                    break;
                case 1:
                    key = _intensityKey;
                    finalValue = _intensityVariation[Random.Range(0, _intensityVariation.Length)];
                    break;
                case 2:
                    key = _colorKey;
                    finalColor = _colorVariation[Random.Range(0, _colorVariation.Length)];
                    break;
            }
            while (count < 1)
            {
                count += Time.fixedDeltaTime / _interpolationDuration;
                switch (type)
                {
                    case 0:
                        _light.range = Mathf.Lerp(_light.range, finalValue, count);
                        break;
                    case 1:
                        _light.intensity = Mathf.Lerp(_light.intensity, finalValue, count);
                        break;
                    case 2:
                        _light.color = Color.Lerp(_light.color, finalColor, count);
                        break;
                }
                yield return delay;
            }
            _interpolationList.Remove(key);
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if(_interpolationDuration == 0)
            {
                _interpolateColor = false;
                _interpolateRange = false;
                _interpolateIntensity = false;
            }
        }
#endif
    }
}