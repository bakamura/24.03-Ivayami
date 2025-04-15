using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using Ivayami.Audio;

namespace Ivayami.Puzzle {
    public class HeavyObjectSounds : EntitySound {

        [SerializeField] private EventReference[] _collectSoundReference;
        private EventInstance[] _collectSoundInstance;
        [SerializeField] private EventReference[] _placeSoundReference;
        private EventInstance[] _placeSoundInstance;

        private void Awake() {
            _collectSoundInstance = new EventInstance[_collectSoundReference.Length];
            for (int i = 0; i < _collectSoundInstance.Length; i++) _collectSoundInstance[i] = InstantiateEvent(_collectSoundReference[i]);
            _placeSoundInstance = new EventInstance[_placeSoundReference.Length];
            for (int i = 0; i < _placeSoundReference.Length; i++) _placeSoundInstance[i] = InstantiateEvent(_placeSoundReference[i]);
        }

        private void Start() {
            GetComponent<HeavyObjectPlacement>().onCollectInstance.AddListener(CollectSound);
        }

        private void CollectSound(bool isCollecting) {
            EventInstance[] usedInstance = isCollecting ? _collectSoundInstance : _placeSoundInstance;
            PlayOneShot(usedInstance[Random.Range(0, usedInstance.Length)], false, Range.Empty);
        }

    }
}
