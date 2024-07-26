using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Ivayami.Audio
{
    public class SoundEffectTrigger : EntitySound
    {
        [SerializeField] private EventReference _audioToPlay;

        private EventInstance _soundInstance;

        [ContextMenu("PlayAudio")]
        public void Play()
        {
            if (Setup())
            {
                PlayOneShot(_soundInstance);
            }
        }

        private bool Setup()
        {
            if (!_audioToPlay.IsNull)
            {
                _soundInstance = InstantiateEvent(_audioToPlay);
                return true;
            }
            return false;
        }
    }
}