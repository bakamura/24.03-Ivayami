using UnityEngine;

namespace Ivayami.Audio
{
    public class PlayerStepSoundArea : MonoBehaviour
    {
        [SerializeField] private PlayerSounds.GroundTypes _groundType;
        public void AddSound()
        {
            PlayerSoundsSingleton.Instance.AddStepToPlaylist(_groundType);
        }

        public void RemoveSound()
        {
            PlayerSoundsSingleton.Instance.RemoveStepFromPlaylist(_groundType);
        }
    }
}