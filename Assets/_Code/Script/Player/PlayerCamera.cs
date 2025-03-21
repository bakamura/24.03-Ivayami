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
        [SerializeField] private float _orbitChangeDuration;

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
            FreeLookCam.m_Orbits = orbits ?? _defaultOrbits;
        }



        //private IEnumerator ChangeOrbitInterpolate() {
        //    float f = 0;
        //    int i;
        //    while (f < 1f) {
        //        f += Time.deltaTime / _orbitChangeDuration;

        //        for (i = 0; i < 3; i++) {
        //            FreeLookCam.m_Orbits[i].m_Height = f;
        //            FreeLookCam.m_Orbits[i].m_Radius = f;
        //        }
        //    }
        //}

    }
}