using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Ivayami.Audio
{
    public class DialogueSounds : EntitySound
    {
        [SerializeField] private EventReference _continueDialogueSound;
        [SerializeField] private bool _debugLog;

        private EventInstance _continueDialogueSoundInstance;

        public enum SoundTypes
        {
            ContinueDialogue
        }
        private void Awake()
        {
            _continueDialogueSoundInstance = InstantiateEvent(_continueDialogueSound);
        }

        public void PlaySound(SoundTypes soundType)
        {
            if (_debugLog) Debug.Log($"PlaySound {soundType}");
            switch (soundType)
            {
                case SoundTypes.ContinueDialogue:
                    PlayOneShot(_continueDialogueSoundInstance, false, Range.Empty);
                    break;
            }
        }

        private void OnDisable()
        {
            if (_continueDialogueSoundInstance.isValid())
            {
                _continueDialogueSoundInstance.getPlaybackState(out PLAYBACK_STATE state);
                if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING) _continueDialogueSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _continueDialogueSoundInstance.release();
            }
        }
    }
}