using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Ivayami.Audio
{
    public class InteractableSounds : EntitySound
    {
        [SerializeField] private bool _debugLog;
        [SerializeField] private EventReference _interactSoundReference;
        [SerializeField] private EventReference _interactReturnSoundReference;
        [SerializeField] private EventReference _activateSoundReference;
        [SerializeField] private EventReference _deactivateSoundReference;
        //[SerializeField] private EventReference _collectSoundReference;
        [SerializeField, Tooltip("When Puzzle Fails")] private EventReference _actionFailedSoundReference;
        [SerializeField, Tooltip("When Puzzle Completes")] private EventReference _actionSuccessSoundReference;

        private EventInstance _interactSoundInstance;
        private EventInstance _interactReturnSoundInstance;
        private EventInstance _activateSoundInstance;
        private EventInstance _deactivateSoundInstance;
        //private EventInstance _collectSoundInstance;
        private EventInstance _actionFailedSoundInstance;
        private EventInstance _actionSuccessSoundInstance;

        private bool _hasDoneSetup;

        public enum SoundTypes
        {
            Interact,
            InteractReturn,
            Activate,
            Deactivate,
            //Collect,
            ActionFailed,
            ActionSuccess
        }

        public void PlaySound(SoundTypes soundType)
        {
            Setup();
            if (_debugLog) Debug.Log($"Interactable {name} is playing soun {soundType}");
            switch (soundType)
            {
                case SoundTypes.Interact:
                    PlayOneShot(_interactSoundInstance);
                    break;
                case SoundTypes.InteractReturn:
                    PlayOneShot(_interactReturnSoundInstance);
                    break;
                case SoundTypes.Activate:
                    PlayOneShot(_activateSoundInstance);
                    break;
                case SoundTypes.Deactivate:
                    PlayOneShot(_deactivateSoundInstance);
                    break;
                //case SoundTypes.Collect:
                //    PlayOneShot(_collectSoundInstance);
                //    break;
                case SoundTypes.ActionFailed:
                    PlayOneShot(_actionFailedSoundInstance);
                    break;
                case SoundTypes.ActionSuccess:
                    PlayOneShot(_actionSuccessSoundInstance);
                    break;
            }
        }

        private void Setup()
        {
            if (!_hasDoneSetup)
            {
                if (!_interactSoundReference.IsNull) _interactSoundInstance = InstantiateEvent(_interactSoundReference);
                if (!_interactReturnSoundReference.IsNull) _interactReturnSoundInstance = InstantiateEvent(_interactReturnSoundReference);
                if (!_activateSoundReference.IsNull) _activateSoundInstance = InstantiateEvent(_activateSoundReference);
                if (!_deactivateSoundReference.IsNull) _deactivateSoundInstance = InstantiateEvent(_deactivateSoundReference);
                //if (!_collectSoundReference.IsNull) _collectSoundInstance = InstantiateEvent(_collectSoundReference);
                if (!_actionFailedSoundReference.IsNull) _actionFailedSoundInstance = InstantiateEvent(_actionFailedSoundReference);
                if (!_actionSuccessSoundReference.IsNull) _actionSuccessSoundInstance = InstantiateEvent(_actionSuccessSoundReference);
                _hasDoneSetup = true;
            }
        }

        private void OnDestroy()
        {
            if (_interactSoundInstance.isValid()) _interactSoundInstance.release();
            if (_interactReturnSoundInstance.isValid()) _interactReturnSoundInstance.release();
            if (_activateSoundInstance.isValid()) _activateSoundInstance.release();
            if (_deactivateSoundInstance.isValid()) _deactivateSoundInstance.release();
            if (_actionFailedSoundInstance.isValid()) _actionFailedSoundInstance.release();
            if (_actionSuccessSoundInstance.isValid()) _actionSuccessSoundInstance.release();
        }
    }
}