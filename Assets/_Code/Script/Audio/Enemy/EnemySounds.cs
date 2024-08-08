using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Ivayami.Audio
{
    public class EnemySounds : EntitySound
    {
        [SerializeField] private EventReference _targetDetectedSound;
        [SerializeField] private EventReference _takeDamageSound;
        [SerializeField] private bool _debugLog;

        private EventInstance _targetDetectedSoundInstance;
        private EventInstance _takeDamageSoundInstance;

        public enum SoundTypes
        {
            TargetDetected,
            TakeDamage
        }

        private void Awake()
        {
            if (!_targetDetectedSound.IsNull) _targetDetectedSoundInstance = InstantiateEvent(_targetDetectedSound);
            if (!_takeDamageSound.IsNull) _takeDamageSoundInstance = InstantiateEvent(_takeDamageSound);
        }

        public void PlaySound(SoundTypes soundType)
        {
            if (_debugLog) Debug.Log($"PlaySound {soundType}");
            switch (soundType)
            {
                case SoundTypes.TargetDetected:
                    PlayOneShot(_targetDetectedSoundInstance);
                    break;
                case SoundTypes.TakeDamage:
                    PlayOneShot(_takeDamageSoundInstance);
                    break;
            }
        }
    }
}