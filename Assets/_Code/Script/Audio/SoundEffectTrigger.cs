using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Ivayami.Audio
{
    public class SoundEffectTrigger : EntitySound
    {
        [SerializeField] private EventReference _audioToPlay;
        [SerializeField] private bool _playOnStart;

        private EventInstance _soundInstance;

        private void Start()
        {
            if (_playOnStart) Play();
        }

        [ContextMenu("Play")]
        public void Play()
        {
            if (Setup())
            {
                PlayOneShot(_soundInstance);
            }
        }
        [ContextMenu("Pause")]
        public void Pause()
        {
            _soundInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STOPPED)
            {
                _soundInstance.getPaused(out bool paused);
                _soundInstance.setPaused(!paused);
            }
        }
        [ContextMenu("Stop")]
        public void Stop()
        {
            _soundInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING) _soundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }

        private bool Setup()
        {
            if (!_audioToPlay.IsNull && !_soundInstance.isValid())
            {
                _soundInstance = InstantiateEvent(_audioToPlay);
                return true;
            }
            return false;
        }

        private void OnDestroy()
        {
            if (_soundInstance.isValid()) 
                _soundInstance.release();
        }
    }
}