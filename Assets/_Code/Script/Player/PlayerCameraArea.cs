using UnityEngine;
using System;
using System.Collections;

namespace Ivayami.Player
{
    public class PlayerCameraArea : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _cameraDistance = .25f;
        [SerializeField] private float[] _camerasRadius;
        [SerializeField] private float _radiusLerpDuration = .5f;
        private bool _targetInside;
        private float[] _defaultRadius = new float[3];
        private Coroutine _interpolationCoroutine;
        private void OnTriggerEnter(Collider other)
        {
            CameraAimReposition.Instance.SetMaxDistance(_cameraDistance);
            for(int i = 0; i < PlayerCamera.Instance.FreeLookCam.m_Orbits.Length; i++)
            {
                _defaultRadius[i] = PlayerCamera.Instance.FreeLookCam.m_Orbits[i].m_Radius;                
            }
            UpdateCameraRadius(_camerasRadius);
            _targetInside = true;
        }

        private void OnTriggerExit(Collider other)
        {
            ResetToDefault();
            _targetInside = false;
        }

        private void OnDisable()
        {
            if (_targetInside) ResetToDefault();
        }

        private void UpdateCameraRadius(float[] radius)
        {
            if(_interpolationCoroutine != null)
            {
                StopCoroutine(_interpolationCoroutine);
                _interpolationCoroutine = null;
            }
           _interpolationCoroutine = StartCoroutine(InterpolateRadiusCoroutine(radius));
        }

        private IEnumerator InterpolateRadiusCoroutine(float[] radius)
        {
            float count = 0;
            while(count < 1)
            {
                count += Time.deltaTime / _radiusLerpDuration;
                for (int i = 0; i < PlayerCamera.Instance.FreeLookCam.m_Orbits.Length; i++)
                {
                    PlayerCamera.Instance.FreeLookCam.m_Orbits[i].m_Radius = 
                        Mathf.MoveTowards(PlayerCamera.Instance.FreeLookCam.m_Orbits[i].m_Radius, radius[i], Time.deltaTime / _radiusLerpDuration);
                }
                yield return null;
            }
            _interpolationCoroutine = null;
        }

        private void ResetToDefault()
        {
            CameraAimReposition.Instance.SetMaxDistance(-1);
            UpdateCameraRadius(_defaultRadius);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            float[] temp = _camerasRadius;
            Array.Resize(ref _camerasRadius, 3);
            for(int i = 0; i < 3; i++)
            {
                _camerasRadius[i] = temp[i];
            }
        }
#endif
    }
}