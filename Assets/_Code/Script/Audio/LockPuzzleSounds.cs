using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Ivayami.Audio
{
    public class LockPuzzleSounds : EntitySound
    {
        [SerializeField] private bool _debugLog;
        [SerializeField] private EventReference _changeOptionSoundReference;
        [SerializeField] private EventReference _confirmOptionSoundReference;

        private EventInstance _changeOptionSoundInstance;
        private EventInstance _confirmOptionSoundInstance;

        private bool _hasDoneSetup;

        public enum SoundTypes
        {
            ChangeOption,
            ConfirmOption
        }

        public void PlayChangeOption()
        {
            PlaySound(SoundTypes.ChangeOption);
        }

        public void PlayConfirmOption()
        {
            PlaySound(SoundTypes.ConfirmOption);
        }

        public void PlaySound(SoundTypes soundType)
        {
            Setup();
            if (_debugLog) Debug.Log($"Interactable {name} is playing soun {soundType}");
            switch (soundType)
            {
                case SoundTypes.ChangeOption:
                    PlayOneShot(_changeOptionSoundInstance, false, Range.Empty);
                    break;
                case SoundTypes.ConfirmOption:
                    PlayOneShot(_confirmOptionSoundInstance, false, Range.Empty);
                    break;
            }
        }

        private void Setup()
        {
            if (!_hasDoneSetup)
            {
                if (!_changeOptionSoundReference.IsNull) _changeOptionSoundInstance = InstantiateEvent(_changeOptionSoundReference);
                if (!_confirmOptionSoundReference.IsNull) _confirmOptionSoundInstance = InstantiateEvent(_confirmOptionSoundReference);
                _hasDoneSetup = true;
            }
        }

        private void OnDisable()
        {
            //PLAYBACK_STATE state;
            if (_changeOptionSoundInstance.isValid())
            {
                //_changeOptionSoundInstance.getPlaybackState(out state);
                //if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING) _changeOptionSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _changeOptionSoundInstance.release();
            }
            if (_confirmOptionSoundInstance.isValid())
            {
                //_confirmOptionSoundInstance.getPlaybackState(out state);
                //if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING) _confirmOptionSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _confirmOptionSoundInstance.release();
            }
            _hasDoneSetup = false;
        }
    }
}