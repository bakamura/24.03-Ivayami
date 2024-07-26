using System.Collections;
using UnityEngine;

namespace Ivayami.Puzzle
{
    public sealed class InterpolateFogShader : MonoBehaviour
    {
        [SerializeField] private AnimationCurve _interpolationCurve;
        [SerializeField, Min(0f)] private float _duration;
        [SerializeField, Range(1e-5f, .2f)] private float _finalValue = 0.0125f;

        private Material _fogMaterial;
        private Coroutine _interpolationCoroutine;
        private Vector4 _initialValue;
        private static readonly int PARAMETER = Shader.PropertyToID("_SoftParticleFadeParams");

        [ContextMenu("Start")]
        public void StartLerp()
        {
            GetMaterialInstance();
            if (_interpolationCoroutine == null) StopLerp();
            _initialValue = _fogMaterial.GetVector(PARAMETER);
            _interpolationCoroutine = StartCoroutine(InterpolateCoroutine());
        }
        [ContextMenu("Stop")]
        public void StopLerp()
        {
            if (_interpolationCoroutine != null)
            {
                StopCoroutine(_interpolationCoroutine);
                _interpolationCoroutine = null;
                _fogMaterial.SetVector(PARAMETER, _initialValue);
            }
        }

        private void GetMaterialInstance()
        {
            if (!_fogMaterial) _fogMaterial = Camera.main.GetComponentInChildren<MeshRenderer>().material;
        }

        private IEnumerator InterpolateCoroutine()
        {
            float count = 0;
            while (count < _duration)
            {
                count += Time.deltaTime;
                _fogMaterial.SetVector(PARAMETER, Vector4.Lerp(_initialValue,
                    new Vector4(_initialValue.x, _finalValue, _initialValue.z, _initialValue.w), _interpolationCurve.Evaluate(count / _duration)));
                yield return null;
            }
            _interpolationCoroutine = null;
        }
    }
}