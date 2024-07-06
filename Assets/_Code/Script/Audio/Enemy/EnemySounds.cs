using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Ivayami.Audio
{
    public class EnemySounds : EntitySound
    {
        [SerializeField] private EventReference _targetDetectedSound;
        [SerializeField] private bool _debugLog;

        private EventInstance _targetDetectedSoundInstance;

        public enum SoundTypes
        {
            TargetDetected
        }

        private void Awake()
        {
            if(!_targetDetectedSound.IsNull)_targetDetectedSoundInstance = InstantiateEvent(_targetDetectedSound);
        }

        public void PlaySound(SoundTypes soundType)
        {
            if(_debugLog) Debug.Log($"PlaySound {soundType}");
            switch (soundType)
            {
                case SoundTypes.TargetDetected:
                    PlayOneShot(_targetDetectedSoundInstance);
                    break;
            }
        }
    }
}