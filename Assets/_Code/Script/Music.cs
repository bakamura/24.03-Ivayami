using FMODUnity;
using FMOD.Studio;
using Paranapiacaba.Player;
using UnityEngine;
using System.Collections;

namespace Paranapiacaba.Audio {
    public class Music : MonoSingleton<Music> {

        [Header("Parameters")]

        [SerializeField] private float _musicReplayDelayMin;
        [SerializeField] private float _musicReplayDelayMax;

        [Header("Cache")]

        private EventInstance _musicInstanceCurrent;
        private float _volume;

        private void Start() {
            PlayerStress.Instance.onStressChange.AddListener(UpdateMusicToStress);
        }

        public void SetMusic(EventReference musicEventRef) {
            if (!musicEventRef.IsNull) {
                _musicInstanceCurrent = RuntimeManager.CreateInstance(musicEventRef);
                SetVolume(_volume);

                StopAllCoroutines();
                _musicInstanceCurrent.setCallback((eventCallback, a, b) => { StartCoroutine(ReplayMusicAfterDelay()); return FMOD.RESULT.OK; }, EVENT_CALLBACK_TYPE.STOPPED);
            }
        }

        private void UpdateMusicToStress(float stress) {
            _musicInstanceCurrent.setParameterByName("Stress", stress);
        }

        private IEnumerator ReplayMusicAfterDelay() {
            yield return new WaitForSeconds(Random.Range(_musicReplayDelayMin, _musicReplayDelayMax));

            _musicInstanceCurrent.start();
        }

        public void SetVolume(float volume) {
            _volume = volume;
            _musicInstanceCurrent.setVolume(_volume);
        }

    }
}