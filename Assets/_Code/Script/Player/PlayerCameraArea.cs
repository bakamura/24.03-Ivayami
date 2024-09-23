using UnityEngine;
using System;
using System.Collections;

namespace Ivayami.Player
{
    public class PlayerCameraArea : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _cameraDistance = .3f;
        [SerializeField] private bool _changeCameraRadius;
        [SerializeField] private float[] _camerasRadius = new float[3];
        [SerializeField] private float _radiusLerpDuration = .5f;
        [SerializeField] private bool _changeCameraHeight;
        [SerializeField] private float[] _camerasHeight = new float[3];
        [SerializeField] private float _heightLerpDuration = .5f;
        private bool _targetInside;
        private float[] _defaultRadius = new float[3];
        private float[] _defaultHeight = new float[3];
        private Coroutine _radiusInterpolationCoroutine;
        private Coroutine _heightInterpolationCoroutine;

        private void OnTriggerEnter(Collider other)
        {
            CameraAimReposition.Instance.SetMaxDistance(_cameraDistance);
            if (_changeCameraRadius || _changeCameraHeight)
            {
                for (int i = 0; i < PlayerCamera.Instance.FreeLookCam.m_Orbits.Length; i++)
                {
                    _defaultRadius[i] = PlayerCamera.Instance.FreeLookCam.m_Orbits[i].m_Radius;
                    _defaultHeight[i] = PlayerCamera.Instance.FreeLookCam.m_Orbits[i].m_Height;
                }
                if (_changeCameraRadius) UpdateCameraRadius(_camerasRadius);
                if (_changeCameraHeight) UpdateCameraHeight(_camerasHeight);
            }
            _targetInside = true;
        }

        private void OnTriggerExit(Collider other)
        {
            CameraAimReposition.Instance.SetMaxDistance(-1);
            if (_changeCameraRadius) UpdateCameraRadius(_defaultRadius);
            if (_changeCameraHeight) UpdateCameraHeight(_defaultHeight);
            _targetInside = false;
        }

        private void OnDisable()
        {
            if (_targetInside)
            {
                CameraAimReposition.Instance.SetMaxDistance(-1);
                if (_changeCameraRadius) ResetOrbitValuesInstant();
                if (_changeCameraHeight) ResetHeightValuesInstant();
                _targetInside = false;
            }
        }

        private void UpdateCameraRadius(float[] radius)
        {
            if (_radiusInterpolationCoroutine != null)
            {
                StopCoroutine(_radiusInterpolationCoroutine);
                _radiusInterpolationCoroutine = null;
            }
            _radiusInterpolationCoroutine = StartCoroutine(InterpolateRadiusCoroutine(radius));
        }

        private void UpdateCameraHeight(float[] heights)
        {
            if (_heightInterpolationCoroutine != null)
            {
                StopCoroutine(_heightInterpolationCoroutine);
                _heightInterpolationCoroutine = null;
            }
            _heightInterpolationCoroutine = StartCoroutine(InterpolateHeightCoroutine(heights));
        }

        private IEnumerator InterpolateRadiusCoroutine(float[] radius)
        {
            float count = 0;
            while (count < 1)
            {
                count += Time.deltaTime / _radiusLerpDuration;
                for (int i = 0; i < PlayerCamera.Instance.FreeLookCam.m_Orbits.Length; i++)
                {
                    PlayerCamera.Instance.FreeLookCam.m_Orbits[i].m_Radius =
                        Mathf.MoveTowards(PlayerCamera.Instance.FreeLookCam.m_Orbits[i].m_Radius, radius[i], Time.deltaTime / _radiusLerpDuration);
                }
                yield return null;
            }
            _radiusInterpolationCoroutine = null;
        }

        private IEnumerator InterpolateHeightCoroutine(float[] heights)
        {
            float count = 0;
            while (count < 1)
            {
                count += Time.deltaTime / _heightLerpDuration;
                for (int i = 0; i < PlayerCamera.Instance.FreeLookCam.m_Orbits.Length; i++)
                {
                    PlayerCamera.Instance.FreeLookCam.m_Orbits[i].m_Height =
                        Mathf.MoveTowards(PlayerCamera.Instance.FreeLookCam.m_Orbits[i].m_Height, heights[i], Time.deltaTime / _heightLerpDuration);
                }
                yield return null;
            }
            _heightInterpolationCoroutine = null;
        }

        private void ResetOrbitValuesInstant()
        {
            for (int i = 0; i < PlayerCamera.Instance.FreeLookCam.m_Orbits.Length; i++)
            {
                PlayerCamera.Instance.FreeLookCam.m_Orbits[i].m_Radius = _defaultRadius[i];
            }
        }

        private void ResetHeightValuesInstant()
        {
            for (int i = 0; i < PlayerCamera.Instance.FreeLookCam.m_Orbits.Length; i++)
            {
                PlayerCamera.Instance.FreeLookCam.m_Orbits[i].m_Height = _defaultHeight[i];
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            float[] temp = _camerasRadius;
            Array.Resize(ref _camerasRadius, 3);
            for (int i = 0; i < temp.Length && i < _camerasRadius.Length; i++)
            {
                _camerasRadius[i] = temp[i];
            }
        }
#endif
    }
}