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
        private bool _hasDoneSetup;

        public enum SoundTypes
        {
            TargetDetected,
            TakeDamage
        }

        public void PlaySound(SoundTypes soundType)
        {
            Setup();
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
        private void Setup()
        {
            if (!_hasDoneSetup)
            {
                if (!_targetDetectedSound.IsNull) _targetDetectedSoundInstance = InstantiateEvent(_targetDetectedSound);
                if (!_takeDamageSound.IsNull) _takeDamageSoundInstance = InstantiateEvent(_takeDamageSound);
                _hasDoneSetup = true;
            }
        }

        private void OnDestroy()
        {
            if (_targetDetectedSoundInstance.isValid()) _targetDetectedSoundInstance.release();
            if (_takeDamageSoundInstance.isValid()) _takeDamageSoundInstance.release();
        }
    }
}