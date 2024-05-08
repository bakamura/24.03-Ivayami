using System.Collections;
using UnityEngine;

namespace Ivayami.Puzzle
{
    public class LerpFogShader : MonoBehaviour
    {
        [SerializeField] private AnimationCurve _interpolationCurve;
        [SerializeField, Min(0f)] private float _duration;
        [SerializeField, Min(0f)] private float _finalValue;

        private Material _fogMaterial;
        private Coroutine _interpolationCoroutine;
        private float _initialValue;
        private static readonly int PARAMETER = Shader.PropertyToID("_SoftParticlesFarFadeDistance");

        [ContextMenu("Start")]
        public void StartLerp()
        {
            GetMaterialInstance();
            if (_interpolationCoroutine == null) StopLerp();
            _initialValue = _fogMaterial.GetFloat(PARAMETER);
            _interpolationCoroutine = StartCoroutine(InterpolateCoroutine());
        }
        [ContextMenu("Stop")]
        public void StopLerp()
        {
            if(_interpolationCoroutine != null)
            {
                StopCoroutine(_interpolationCoroutine);
                _interpolationCoroutine = null;
                _fogMaterial.SetFloat(PARAMETER, _initialValue);
            }
        }

        private void GetMaterialInstance()
        {
            if (!_fogMaterial) _fogMaterial = Camera.main.GetComponentInChildren<MeshRenderer>().material;
        }

        private IEnumerator InterpolateCoroutine()
        {
            float count = 0;
            while(count < _duration)
            {
                count += Time.deltaTime;
                _fogMaterial.SetFloat(PARAMETER, Mathf.Lerp(_initialValue, _finalValue, _interpolationCurve.Evaluate(count / _duration)));
                Debug.Log(_fogMaterial.GetFloat(PARAMETER));
                yield return null;
            }
            _interpolationCoroutine = null;
        }
    }
}