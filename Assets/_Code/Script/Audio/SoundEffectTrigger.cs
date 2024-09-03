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

        public void Pause()
        {
            _soundInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STOPPED)
            {
                _soundInstance.getPaused(out bool paused);
                _soundInstance.setPaused(!paused);
            }
        }

        public void Stop()
        {
            _soundInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING) _soundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
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