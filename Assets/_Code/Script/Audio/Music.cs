using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using System.Collections;
using Ivayami.Player;
using Ivayami.Save;

namespace Ivayami.Audio {
    public class Music : MonoSingleton<Music> {

        [Header("Parameters")]

        [SerializeField] private float _musicReplayDelayMin;
        [SerializeField] private float _musicReplayDelayMax;

        [Header("Cache")]

        private EventInstance _musicInstanceCurrent;

        private void Start() {
            PlayerStress.Instance.onStressChange.AddListener(UpdateMusicToStress);
        }

        public void SetMusic(EventReference musicEventRef) {
            if (!musicEventRef.IsNull) {
                _musicInstanceCurrent.release();
                _musicInstanceCurrent = RuntimeManager.CreateInstance(musicEventRef);
                _musicInstanceCurrent.setVolume(SaveSystem.Instance.Options.musicVol);

                StopAllCoroutines();
                _musicInstanceCurrent.setCallback((eventCallback, a, b) => { StartCoroutine(ReplayMusicAfterDelay()); return FMOD.RESULT.OK; }, EVENT_CALLBACK_TYPE.STOPPED);
                _musicInstanceCurrent.start();
            }
        }

        public void Stop() {
            _musicInstanceCurrent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _musicInstanceCurrent.release();
        }

        private void UpdateMusicToStress(float stress) {
            _musicInstanceCurrent.setParameterByName("Stress", stress);
        }

        private IEnumerator ReplayMusicAfterDelay() {
            yield return new WaitForSeconds(Random.Range(_musicReplayDelayMin, _musicReplayDelayMax));

            _musicInstanceCurrent.start();
        }

        public void SetVolume(float volume) {
            _musicInstanceCurrent.setVolume(volume);
        }

    }
}