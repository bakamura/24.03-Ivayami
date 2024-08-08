using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Ivayami.Player;
using Ivayami.Save;
using Ivayami.UI;

namespace Ivayami.Audio {
    public class Music : MonoSingleton<Music> {

        [Header("Parameters")]


        [SerializeField, Min(0f)] private float _musicPlayDelayMin;
        [SerializeField, Min(0f)] private float _musicPlayDelayMax;

        [Space(24)]

        [SerializeField, Min(0f)] private float _musicReplayDelayMin;
        [SerializeField, Min(0f)] private float _musicReplayDelayMax;

        [Space(24)]

        [SerializeField, Min(0f)] private float _fadeOutDuration;
        [SerializeField, Min(0f)] private float _fadeInDuration;

        private bool _shouldDelayToRepeat = true;

        [Header("Cache")]

        private EventInstance _musicInstanceCurrent;
        private Coroutine _fadeOutRoutine;

        private void Start() {
            PlayerStress.Instance.onStressChange.AddListener(UpdateMusicToStress);
            StartCoroutine(RandomlyMuteMusic());
        }

        public void SetMusic(EventReference musicEventRef) {
            if (!musicEventRef.IsNull) {
                if (_fadeOutRoutine != null) {
                    StopCoroutine(_fadeOutRoutine);
                    _fadeOutRoutine = null;
                }
                _musicInstanceCurrent.getParameterByName("Stress", out float stress);
                _musicInstanceCurrent.release();
                _musicInstanceCurrent.setParameterByName("Stress", stress); //

                _musicInstanceCurrent = RuntimeManager.CreateInstance(musicEventRef);
                _musicInstanceCurrent.setVolume(SaveSystem.Instance.Options.musicVol);

                _musicInstanceCurrent.start();
            }
        }

        public void Stop() {
            _fadeOutRoutine = StartCoroutine(StopRoutine());
        }

        private IEnumerator StopRoutine() {
            float fadeOut = 1;
            while (fadeOut > 0) {
                fadeOut -= Time.deltaTime / _fadeOutDuration;
                if (fadeOut < 0) fadeOut = 0f;
                _musicInstanceCurrent.setVolume(fadeOut * SaveSystem.Instance.Options.musicVol);

                yield return null;
            }
            _musicInstanceCurrent.release();
            _fadeOutRoutine = null;
        }

        public void ForceStop() {
            _musicInstanceCurrent.release();
        }

        private void UpdateMusicToStress(float stress) {
            _musicInstanceCurrent.setParameterByName("Stress", stress);
        }

        private IEnumerator RandomlyMuteMusic() {
            float fade;
            while (true) {
                yield return new WaitForSeconds(Random.Range(_musicPlayDelayMin, _musicPlayDelayMax));

                fade = 1;
                while (fade > 0) {
                    fade -= Time.deltaTime / _fadeOutDuration;
                    if (fade < 0) fade = 0f;
                    _musicInstanceCurrent.setVolume(fade * SaveSystem.Instance.Options.musicVol);

                    yield return null;
                }

                yield return new WaitForSeconds(Random.Range(_musicPlayDelayMin, _musicPlayDelayMax));

                fade = 0;
                while (fade < 1) {
                    fade += Time.deltaTime / _fadeInDuration;
                    if (fade > 1) fade = 1f;
                    _musicInstanceCurrent.setVolume(fade * SaveSystem.Instance.Options.musicVol);

                    yield return null;
                }

            }
        }

        public void ShouldDelayToRepeat(bool should) {
            _shouldDelayToRepeat = should;
        }

        public void VolumeUpdate(float volume) {
            _musicInstanceCurrent.getVolume(out float currentVolume);
            _musicInstanceCurrent.setVolume(currentVolume * volume / SaveSystem.Instance.Options.musicVol);
        }

    }
}