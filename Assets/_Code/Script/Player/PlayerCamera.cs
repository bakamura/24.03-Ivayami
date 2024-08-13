using Cinemachine;

namespace Ivayami.Player {
    public class PlayerCamera : MonoSingleton<PlayerCamera> {

        public CinemachineFreeLook FreeLookCam { get; private set; }

        protected override void Awake() {
            base.Awake();
            
            FreeLookCam = GetComponent<CinemachineFreeLook>();
        }

        public void SetSensitivityX(float sensitivityX) {
            FreeLookCam.m_XAxis.m_MaxSpeed = sensitivityX;
        }

        public void SetSensitivityY(float sensitivityY) {
            FreeLookCam.m_YAxis.m_MaxSpeed = sensitivityY;
        }

    }
}