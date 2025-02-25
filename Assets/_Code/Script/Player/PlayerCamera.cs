using Cinemachine;
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

        protected override void Awake() {
            base.Awake();
            
            FreeLookCam = GetComponent<CinemachineFreeLook>();
            InputProvider = GetComponent<CinemachineInputProvider>();
            MainCamera = Camera.main;
            CinemachineBrain = MainCamera.GetComponent<CinemachineBrain>();
        }

        public void SetSensitivityX(float sensitivityX) {
            FreeLookCam.m_XAxis.m_MaxSpeed = sensitivityX;
        }

        public void SetSensitivityY(float sensitivityY) {
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

    }
}