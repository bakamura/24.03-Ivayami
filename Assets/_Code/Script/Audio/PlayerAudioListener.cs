using FMODUnity;
using Ivayami.Player;

namespace Ivayami.Audio
{
    public class PlayerAudioListener : MonoSingleton<PlayerAudioListener>
    {
        private StudioListener _playerAudioListener;
        private StudioListener _cameraAudioListener;
        private void Start()
        {
            _playerAudioListener = PlayerMovement.Instance.GetComponentInChildren<StudioListener>();
            _cameraAudioListener = PlayerCamera.Instance.MainCamera.GetComponentInChildren<StudioListener>();
        }

        public void UpdateAudioSource(bool isPlayerSourceActive)
        {
            _playerAudioListener.enabled = isPlayerSourceActive;
            _cameraAudioListener.enabled = !isPlayerSourceActive;
        }
    }
}