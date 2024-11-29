using System.Collections;
using UnityEngine;
using Ivayami.Player;

namespace Ivayami.Puzzle
{
    public sealed class InterpolateFogShader : MonoBehaviour
    {
        [SerializeField] private AnimationCurve _interpolationCurve;
        [SerializeField, Min(0f)] private float _duration;
        [SerializeField, Range(1e-5f, .2f), Tooltip("Big values means more fog, small values means less fog")] private float _finalValue = 0.03333f;

        private static Material _fogMaterial;
        private Coroutine _interpolationCoroutine;
        private Vector4 _initialValue;
        private static readonly int PARAMETER = Shader.PropertyToID("_SoftParticleFadeParams");

        [ContextMenu("Start")]
        public void StartLerp()
        {
            if (!gameObject.activeInHierarchy) return;
            GetMaterialInstance();
            StopLerp();
            _initialValue = _fogMaterial.GetVector(PARAMETER);
            _interpolationCoroutine = StartCoroutine(InterpolateCoroutine());
        }
        [ContextMenu("Stop")]
        public void StopLerp()
        {
            if (!gameObject.activeInHierarchy) return;
            if (_interpolationCoroutine != null)
            {
                StopCoroutine(_interpolationCoroutine);
                _interpolationCoroutine = null;
                _fogMaterial.SetVector(PARAMETER, new Vector4(_initialValue.x, _finalValue, _initialValue.z, _initialValue.w));
            }
        }

        private void GetMaterialInstance()
        {
            if (!_fogMaterial) _fogMaterial = PlayerCamera.Instance.MainCamera.GetComponentInChildren<MeshRenderer>().material;
        }

        private IEnumerator InterpolateCoroutine()
        {
            float count = 0;
            if(_duration > 0)
            {
                while (count < _duration)
                {
                    count += Time.deltaTime;
                    _fogMaterial.SetVector(PARAMETER, Vector4.Lerp(_initialValue,
                        new Vector4(_initialValue.x, _finalValue, _initialValue.z, _initialValue.w), _interpolationCurve.Evaluate(count / _duration)));
                    yield return null;
                }                
            }
            else
            {
                _fogMaterial.SetVector(PARAMETER, new Vector4(_initialValue.x, _finalValue, _initialValue.z, _initialValue.w));
            }
            _interpolationCoroutine = null;
        }
    }
}