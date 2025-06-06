using UnityEngine;

namespace Ivayami.Audio
{
    public class PlayerStepSoundArea : MonoBehaviour
    {
        [SerializeField] private PlayerSounds.GroundTypes _groundType;
        public void AddSound()
        {
            if (!PlayerSoundsRef.Instance) return;
            PlayerSoundsRef.Instance.PlayerSounds.AddStepToPlaylist(_groundType);
        }

        public void RemoveSound()
        {
            if (!PlayerSoundsRef.Instance) return;
            PlayerSoundsRef.Instance.PlayerSounds.RemoveStepFromPlaylist(_groundType);
        }
    }
}