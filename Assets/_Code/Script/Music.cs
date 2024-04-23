using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Paranapiacaba.Player;

namespace Paranapiacaba.Audio {
    public class Music : MonoSingleton<Music> {

        [Header("Parameters")]

        [SerializeField] private float _musicReplayDelayMin;
        [SerializeField] private float _musicReplayDelayMax;

        [Header("Cache")]

        private EventInstance _musicInstanceCurrent;
        private float _volume = 0.5f; //

        private void Start() {
            PlayerStress.Instance.onStressChange.AddListener(UpdateMusicToStress);
        }

        private void OnDestroy() {
            _musicInstanceCurrent.release();
        }

        public void SetMusic(EventReference musicEventRef) {
            if (!musicEventRef.IsNull) {
                _musicInstanceCurrent.release();
                _musicInstanceCurrent = RuntimeManager.CreateInstance(musicEventRef);
                SetVolume(_volume);

                StopAllCoroutines();
                _musicInstanceCurrent.setCallback((eventCallback, a, b) => { StartCoroutine(ReplayMusicAfterDelay()); return FMOD.RESULT.OK; }, EVENT_CALLBACK_TYPE.STOPPED);
                _musicInstanceCurrent.start();
                Logger.Log(LogType.Scene, $"Music Started: {musicEventRef.Path}");
            }
            else Logger.Log(LogType.Scene, $"Music Event Reference is null");
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