using UnityEngine;
using Cinemachine;

namespace Ivayami.Player {
    public class PlayerCamera : MonoSingleton<PlayerCamera> {

        [SerializeField] private float _sensitivityMultiplierX;
        [SerializeField] private float _sensitivityMultiplierY;
        public CinemachineFreeLook FreeLookCam { get; private set; }

        protected override void Awake() {
            base.Awake();
            
            FreeLookCam = GetComponent<CinemachineFreeLook>();
        }

        public void SetCameraSensitivity(float sensitivity) {
            FreeLookCam.m_XAxis.m_MaxSpeed = sensitivity * _sensitivityMultiplierX;
            FreeLookCam.m_YAxis.m_MaxSpeed = sensitivity * _sensitivityMultiplierY;
        }

    }
}