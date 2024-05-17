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
                    PlayOneShot(_continueDialogueSoundInstance);
                    break;
            }
        }
    }
}