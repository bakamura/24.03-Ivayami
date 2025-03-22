using Cinemachine;
using System.Collections;
using UnityEngine;

namespace Ivayami.Player {
    public class PlayerCamera : MonoSingleton<PlayerCamera> {

        [SerializeField] private Transform _cameraAimPoint;
        [SerializeField] private Transform _cameraAimRotator;
        public Camera MainCamera { get; private set; }
        public CinemachineBrain CinemachineBrain { get; private set; }
        public CinemachineFreeLook FreeLookCam { get; private set; }
        public CinemachineInputProvider InputProvider { get; private set; }
        public Transform CameraAimPoint => _cameraAimPoint;
        public Transform CameraAimRotator => _cameraAimRotator;

        private CinemachineFreeLook.Orbit[] _defaultOrbits;
        private CinemachineFreeLook.Orbit[] _targetOrbits;
        [SerializeField, Min(0.01f)] private float _orbitChangeDuration;

        protected override void Awake() {
            base.Awake();

            FreeLookCam = GetComponent<CinemachineFreeLook>();
            InputProvider = GetComponent<CinemachineInputProvider>();
            MainCamera = Camera.main;
            CinemachineBrain = MainCamera.GetComponent<CinemachineBrain>();
            _defaultOrbits = FreeLookCam.m_Orbits;
        }

        public void SetSensitivityX(float sensitivityX) {
            FreeLookCam.m_XAxis.m_MaxSpeed = sensitivityX;
        }

        public void SetSensitivityY(float sensitivityY) {
            FreeLookCam.m_YAxis.m_MaxSpeed = sensitivityY;
        }

        public void UpdateCameraControls(bool isActive) {
            InputProvider.enabled = isActive;
        }

        public void InvertCamera(bool isActive) {
            FreeLookCam.m_YAxis.m_InvertInput = isActive;
        }

        public void SetOrbit(CinemachineFreeLook.Orbit[] orbits = null) {
            _targetOrbits = orbits ?? _defaultOrbits;
            StartCoroutine(ChangeOrbitInterpolate());
        }

        private IEnumerator ChangeOrbitInterpolate() {
            float interpolation = 0;
            int i;
            CinemachineFreeLook.Orbit[] orbitsInitial = FreeLookCam.m_Orbits;
            while (interpolation < 1f) {
                interpolation += Time.deltaTime / _orbitChangeDuration;

                for (i = 0; i < 3; i++) {
                    FreeLookCam.m_Orbits[i].m_Height = Mathf.LerpUnclamped(orbitsInitial[i].m_Height, _targetOrbits[i].m_Height, interpolation);
                    FreeLookCam.m_Orbits[i].m_Radius = Mathf.LerpUnclamped(orbitsInitial[i].m_Radius, _targetOrbits[i].m_Radius, interpolation);
                }
                yield return null;
            }
            FreeLookCam.m_Orbits = _targetOrbits;
        }

    }
}