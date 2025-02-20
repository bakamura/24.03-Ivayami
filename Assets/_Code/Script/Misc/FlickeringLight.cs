using System.Collections;
using UnityEngine;

namespace Ivayami.Misc
{
    [RequireComponent(typeof(Light), typeof(Collider))]
    public class FlickeringLight : MonoBehaviour
    {
        [SerializeField] private bool _autoStart;
        [SerializeField] private float _tickFrequency;
        [SerializeField, Range(.01f, 100f)] private float _colorVariationChance;
        [SerializeField] private Color[] _colorVariation;
        [SerializeField, Range(.01f, 100f)] private float _rangeVariationChance;
        [SerializeField, Min(0f)] private float[] _rangeVariation;
        [SerializeField, Range(.01f, 100f)] private float _intensityVariationChance;
        [SerializeField, Min(0f)] private float[] _intensityVariation;

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
        private bool _isActive;

        private void Awake()
        {
            if (_autoStart)
            {
                Activate();
                UpdateCoroutine(true);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            UpdateCoroutine(true);
        }

        private void OnTriggerExit(Collider other)
        {
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
                    _light.range = _rangeVariation[Random.Range(0, _rangeVariation.Length)];
                   // Debug.Log($"Change Range {value}");
                }
                if (_intensityVariation.Length > 0 && val <= _intensityVariationChance)
                {
                    _light.intensity = _intensityVariation[Random.Range(0, _intensityVariation.Length)];
                    //Debug.Log($"Change Intensity {_light.intensity}");
                }
                if (_colorVariation.Length > 0 && val <= _colorVariationChance)
                {
                    _light.color = _colorVariation[Random.Range(0, _colorVariation.Length)];
                    //Debug.Log($"Change Color {_light.color}");
                }

                yield return delay;
            }
        }
    }
}