using UnityEngine;
using Cinemachine;

namespace Ivayami.Player {
    public class PlayerCamera : MonoSingleton<PlayerStress> {

        [SerializeField] private float _sensitivityMultiplierX;
        [SerializeField] private float _sensitivityMultiplierY;
        private CinemachineFreeLook _freeLookCam;

        protected override void Awake() {
            base.Awake();
            
            _freeLookCam = GetComponent<CinemachineFreeLook>();
        }

        public void SetCameraSensitivity(float sensitivity) {
            _freeLookCam.m_XAxis.m_MaxSpeed = sensitivity * _sensitivityMultiplierX;
            _freeLookCam.m_YAxis.m_MaxSpeed = sensitivity * _sensitivityMultiplierY;
        }

    }
}