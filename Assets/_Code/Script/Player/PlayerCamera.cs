using System.Collections;
using UnityEngine;
using Cinemachine;

namespace Ivayami.Player
{
    public class PlayerCamera : MonoSingleton<PlayerCamera>
    {

        [SerializeField] private Transform _cameraAimPoint;
        [SerializeField] private Transform _cameraAimRotator;
        public Camera MainCamera { get; private set; }
        public Camera UICamera { get; private set; }
        public CinemachineBrain CinemachineBrain { get; private set; }
        public CinemachineFreeLook FreeLookCam { get; private set; }
        public CinemachineInputProvider InputProvider { get; private set; }
        public Transform CameraAimPoint => _cameraAimPoint;
        public Transform CameraAimRotator => _cameraAimRotator;

        private CinemachineFreeLook.Orbit[] _defaultOrbits;
        private CinemachineFreeLook.Orbit[] _targetOrbits;
        [SerializeField, Min(0.01f)] private float _orbitChangeDuration;

        public enum CameraBackgroundTypes
        {
            Skybox = 1,
            SolidColor = 2
        }

        protected override void Awake()
        {
            base.Awake();

            FreeLookCam = GetComponent<CinemachineFreeLook>();
            InputProvider = GetComponent<CinemachineInputProvider>();
            MainCamera = Camera.main;
            UICamera = MainCamera.GetComponentsInChildren<Camera>()[1];
            CinemachineBrain = MainCamera.GetComponent<CinemachineBrain>();
            _defaultOrbits = CloneOrbits(FreeLookCam.m_Orbits);
        }

        public void SetSensitivityX(float sensitivityX)
        {
            FreeLookCam.m_XAxis.m_MaxSpeed = sensitivityX;
        }

        public void SetSensitivityY(float sensitivityY)
        {
            FreeLookCam.m_YAxis.m_MaxSpeed = sensitivityY;
        }

        public void UpdateCameraControls(bool isActive)
        {
            InputProvider.enabled = isActive;
        }

        public void InvertCamera(bool isActive)
        {
            FreeLookCam.m_YAxis.m_InvertInput = isActive;
        }

        public void SetOrbits(CinemachineFreeLook.Orbit[] orbits = null)
        {
            _targetOrbits = orbits ?? _defaultOrbits;
            StopAllCoroutines(); //
            StartCoroutine(ChangeOrbitsInterpolate());
        }

        public void SetSkybox(CameraBackgroundTypes backgroundType, Color backgroundColor)
        {
            if (MainCamera.clearFlags != (CameraClearFlags)backgroundType) MainCamera.clearFlags = (CameraClearFlags)backgroundType;
            MainCamera.backgroundColor = backgroundColor;
        }

        private IEnumerator ChangeOrbitsInterpolate()
        {
            float interpolation = 0;
            int i;
            CinemachineFreeLook.Orbit[] orbitsInitial = FreeLookCam.m_Orbits;
            while (interpolation < 1f)
            {
                interpolation += Time.deltaTime / _orbitChangeDuration;

                for (i = 0; i < 3; i++)
                {
                    FreeLookCam.m_Orbits[i].m_Height = Mathf.LerpUnclamped(orbitsInitial[i].m_Height, _targetOrbits[i].m_Height, interpolation);
                    FreeLookCam.m_Orbits[i].m_Radius = Mathf.LerpUnclamped(orbitsInitial[i].m_Radius, _targetOrbits[i].m_Radius, interpolation);
                }
                yield return null;
            }
            FreeLookCam.m_Orbits = CloneOrbits(_targetOrbits);
        }

        private CinemachineFreeLook.Orbit[] CloneOrbits(CinemachineFreeLook.Orbit[] orbits)
        {
            CinemachineFreeLook.Orbit[] clone = new CinemachineFreeLook.Orbit[orbits.Length];
            for (int i = 0; i < clone.Length; i++) clone[i] = orbits[i];
            return clone;
        }

    }
}