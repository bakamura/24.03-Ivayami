using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using Ivayami.Puzzle;

namespace Ivayami.Audio {
    public class HeavyObjectSounds : EntitySound {

        [SerializeField] private EventReference _collectSoundReference;
        private EventInstance _collectSoundInstance;
        [SerializeField] private EventReference _placeSoundReference;
        private EventInstance _placeSoundInstance;

        private void Awake() {
            _collectSoundInstance = InstantiateEvent(_collectSoundReference);
            _placeSoundInstance = InstantiateEvent(_placeSoundReference);
        }

        private void Start() {
            GetComponent<HeavyObjectPlacement>().onCollectInstance.AddListener(CollectSound);
        }

        private void CollectSound(bool isCollecting) {
            PlayOneShot((isCollecting ? _collectSoundInstance : _placeSoundInstance), false, Range.Empty);
        }

    }
}
