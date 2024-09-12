using Cinemachine;
using UnityEngine;

namespace Ivayami.Player {
    public class PlayerCamera : MonoSingleton<PlayerCamera> {

        [SerializeField] private Transform _cameraAimPoint;
        public Camera MainCamera { get; private set; }
        public CinemachineFreeLook FreeLookCam { get; private set; }
        public CinemachineInputProvider InputProvider { get; private set; }
        public Transform CameraAimPoint => _cameraAimPoint;

        protected override void Awake() {
            base.Awake();
            
            FreeLookCam = GetComponent<CinemachineFreeLook>();
            InputProvider = GetComponent<CinemachineInputProvider>();
            MainCamera = Camera.main;
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

    }
}