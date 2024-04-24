using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using Ivayami.Save;

namespace Ivayami.Audio {
    public class UiSound : MonoBehaviour {

        [SerializeField] private EventReference _changeSelectedReference;
        [SerializeField] private EventReference _goForthReference;
        [SerializeField] private EventReference _goBackReference;
        private EventInstance _changeSelectedInstance;
        private EventInstance _goForthInstance;
        private EventInstance _goBackInstance;

        private void Awake() {
            _changeSelectedInstance = InstantiateEvent(_changeSelectedReference);
            _goForthInstance = InstantiateEvent(_goForthReference);
            _goBackInstance = InstantiateEvent(_goBackReference);
        }

        private void PlayOneShot(EventInstance sound) {
            sound.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING) sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            sound.setVolume(SaveSystem.Instance.Options.sfxVol);
            sound.start();
        }

        private EventInstance InstantiateEvent(EventReference sound) {
            EventInstance eventInstance = RuntimeManager.CreateInstance(sound);
            return eventInstance;
        }

        public void ChangeSelected() {
            PlayOneShot(_changeSelectedInstance);

            Logger.Log(LogType.UI, $"Play Sound 'ChangeSelected'");
        }

        public void GoForth() {
            PlayOneShot(_goForthInstance);

            Logger.Log(LogType.UI, $"Play Sound 'GoForth'");
        }

        public void GoBack() {
            PlayOneShot(_goBackInstance);

            Logger.Log(LogType.UI, $"Play Sound 'GoBack'");
        }

    }
}