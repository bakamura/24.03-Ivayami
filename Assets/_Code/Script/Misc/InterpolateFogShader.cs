using System.Collections;
using UnityEngine;
using Ivayami.Player;
using UnityEngine.Serialization;
using PSX;

namespace Ivayami.Misc
{
    public sealed class InterpolateFogShader : MonoBehaviour
    {
        [SerializeField] private AnimationCurve _interpolationCurve;
        [SerializeField, Min(0f)] private float _duration;
        [SerializeField, FormerlySerializedAs("_finalValue"), Range(1e-5f, .2f), Tooltip("Big values means more fog, small values means less fog")] private float _sphericalFogFinalValue = 0.03333f;
        [SerializeField, Range(0f, 50f), Tooltip("Big values means more fog, small values means less fog")] private float _psxFogFinalValue = 15f;
        [SerializeField] private bool _changeColor;
        [SerializeField] private Color _sphericalFogFinalColor =  new Color(0.4622641f, 0.4622641f, 0.4622641f,1f);
        [SerializeField] private Color _psxFogFinalColor =  Color.white;

        private static Material _fogMaterial;
        private Coroutine _interpolationCoroutine;
        private Vector4 _initialSphericalFogValue;
        private float _initialPsxFogValue;
        private Color _initialSphericalFogColor;
        private Color _initialPsxFogColor;
        private static readonly int FOG_SPHERE_DISTANCE = Shader.PropertyToID("_SoftParticleFadeParams");
        private static readonly int FOG_SPHERE_COLOR = Shader.PropertyToID("_BaseColor");

        [ContextMenu("Start")]
        public void StartLerp()
        {
            GetMaterialInstance();
            _initialSphericalFogValue = _fogMaterial.GetVector(FOG_SPHERE_DISTANCE);
            _initialPsxFogValue = PsxManager.Instance.PSXFog.fogDistance.value;
            if (_changeColor)
            {
                _initialPsxFogColor = PsxManager.Instance.PSXFog.fogColor.value;
                _initialSphericalFogColor = _fogMaterial.GetColor(FOG_SPHERE_COLOR);
            }
            if (!gameObject.activeInHierarchy)
            {
                _fogMaterial.SetVector(FOG_SPHERE_DISTANCE, new Vector4(_initialSphericalFogValue.x, _sphericalFogFinalValue, _initialSphericalFogValue.z, _initialSphericalFogValue.w));
                PsxManager.Instance.PSXFog.fogDistance.value = _psxFogFinalValue;
                if (_changeColor)
                {
                    _fogMaterial.SetColor(FOG_SPHERE_COLOR, _sphericalFogFinalColor);
                    PsxManager.Instance.PSXFog.fogColor.value = _psxFogFinalColor;
                }
                return;
            }
            StopLerp();
            _interpolationCoroutine = StartCoroutine(InterpolateCoroutine());
        }
        [ContextMenu("Stop")]
        public void StopLerp()
        {
            //if (!gameObject.activeInHierarchy) return;
            if (_interpolationCoroutine != null)
            {
                StopCoroutine(_interpolationCoroutine);
                _interpolationCoroutine = null;
                //_fogMaterial.SetVector(PARAMETER, new Vector4(_initialValue.x, _finalValue, _initialValue.z, _initialValue.w));
            }
        }

        private void GetMaterialInstance()
        {
            if (!_fogMaterial) _fogMaterial = PlayerCamera.Instance.MainCamera.GetComponentInChildren<MeshRenderer>().material;
        }

        private IEnumerator InterpolateCoroutine()
        {
            float count = 0;
            float evaluate;
            Fog psxFog = PsxManager.Instance.PSXFog;
            if(_duration > 0)
            {
                while (count < _duration)
                {
                    count += Time.deltaTime;
                    evaluate = _interpolationCurve.Evaluate(count / _duration);

                    _fogMaterial.SetVector(FOG_SPHERE_DISTANCE, Vector4.Lerp(_initialSphericalFogValue,
                        new Vector4(_initialSphericalFogValue.x, _sphericalFogFinalValue, _initialSphericalFogValue.z, _initialSphericalFogValue.w), evaluate));
                    psxFog.fogDistance.value = Mathf.Lerp(_initialPsxFogValue, _psxFogFinalValue, evaluate);

                    if (_changeColor)
                    {
                        _fogMaterial.SetColor(FOG_SPHERE_COLOR, Color.Lerp(_initialSphericalFogColor, _sphericalFogFinalColor, evaluate));
                        psxFog.fogColor.value = Color.Lerp(_initialPsxFogColor, _psxFogFinalColor, evaluate);
                    }
                    yield return null;
                }                
            }
            else
            {
                _fogMaterial.SetVector(FOG_SPHERE_DISTANCE, new Vector4(_initialSphericalFogValue.x, _sphericalFogFinalValue, _initialSphericalFogValue.z, _initialSphericalFogValue.w));
                psxFog.fogDistance.value = _psxFogFinalValue;

                if (_changeColor)
                {
                    _fogMaterial.SetColor(FOG_SPHERE_COLOR, _sphericalFogFinalColor);
                    psxFog.fogColor.value = _psxFogFinalColor;
                }
            }
            _interpolationCoroutine = null;
        }
    }
}