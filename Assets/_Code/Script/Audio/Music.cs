using FMOD.Studio;
using FMODUnity;
using Ivayami.Player;
using Ivayami.Save;
using Ivayami.UI;
using System.Collections;
using UnityEngine;

namespace Ivayami.Audio
{
    public class Music : MonoSingleton<Music>
    {
        [Header("Parameters")]

        [SerializeField] private Range _defaultMusicPlayDelayRange;

        [Space(24)]

        [SerializeField] private Range _defaultMusicReplayDelayRange;

        [Space(24)]

        [SerializeField, Min(0f)] private float _fadeOutDuration;
        [SerializeField, Min(0f)] private float _fadeInDuration;

        [Header("Cache")]

        private MusicData _musicCurrent;
        private Coroutine _silenceRoutine;
        private Coroutine _fadeOutRoutine;
        private Coroutine _transitionCoroutine;
        private float _currentVolume;

        // for fade, fade in is the Min and fade out is the Max
        [System.Serializable]
        public struct Range
        {
            [Min(0f)] public float Min;
            [Min(0f)] public float Max;

            public Range(float min, float max)
            {
                Min = min;
                Max = max;
            }
        }

        private struct MusicData
        {
            public EventInstance MusicInstance;
            public Range Fade;
            public Range Replay;
            public Range StartDelay;
            public FMOD.GUID Guid;

            public MusicData(EventInstance eventInstance, Range fade, Range replay, Range startDelay)
            {
                MusicInstance = eventInstance;
                Fade = fade;
                Replay = replay;
                StartDelay = startDelay;
                eventInstance.getDescription(out EventDescription description);
                description.getID(out Guid);
            }
        }

        private void Start()
        {
            StressIndicatorSmoother.Instance.OnStressSmoothed.AddListener(UpdateMusicToStress);
        }

        public void SetMusic(EventReference musicEventRef, bool shouldStopPeriodically, bool useDefaultFade, bool useDefaultReplay, bool useDefaultStartDelay, Range fade = new(), Range replay = new(), Range startDelay = new())
        {
            if (!musicEventRef.IsNull)
            {
                StopCoroutines();

                _transitionCoroutine = StartCoroutine(TransitionMusic(musicEventRef, shouldStopPeriodically, useDefaultFade, useDefaultReplay, useDefaultStartDelay, fade, replay, startDelay));
            }
        }

        public void Stop(FMOD.GUID guid)
        {
            if (guid == _musicCurrent.Guid)
            {
                StopCoroutines();
                _fadeOutRoutine = StartCoroutine(StopRoutine());
            }
        }

        private void StopCoroutines()
        {
            if (_fadeOutRoutine != null)
            {
                StopCoroutine(_fadeOutRoutine);
                _fadeOutRoutine = null;
            }
            if (_silenceRoutine != null)
            {
                StopCoroutine(_silenceRoutine);
                _silenceRoutine = null;
            }
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
                _transitionCoroutine = null;
            }
        }

        private IEnumerator StopRoutine()
        {
            if (_musicCurrent.MusicInstance.isValid())
            {
                float fadeOut = _currentVolume > 0 ? _currentVolume : 1;
                while (fadeOut > 0)
                {
                    fadeOut -= Time.deltaTime / _musicCurrent.Fade.Max;
                    if (fadeOut < 0) _currentVolume = 0f;
                    else _currentVolume = fadeOut * SaveSystem.Instance.Options.musicVol;
                    _musicCurrent.MusicInstance.setVolume(_currentVolume);

                    yield return null;
                }
                _musicCurrent.MusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _musicCurrent.MusicInstance.release();
                _musicCurrent = new MusicData();
            }
            _fadeOutRoutine = null;
        }

        public void ForceStop()
        {
            StopCoroutines();
            _musicCurrent.MusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _musicCurrent.MusicInstance.release();
            _musicCurrent = new MusicData();
        }

        private void UpdateMusicToStress(float stress)
        {
            if (_musicCurrent.MusicInstance.isValid()) _musicCurrent.MusicInstance.setParameterByName("Stress", stress / PlayerStress.Instance.MaxStress);
        }

        private IEnumerator TransitionMusic(EventReference musicEventRef, bool shouldStopPeriodically, bool useDefaultFade, bool useDefaultReplay, bool useDefaultStartDelay, Range fade, Range replay, Range startDelay)
        {
            float crossFade;
            bool isSameMusic = _musicCurrent.Guid == musicEventRef.Guid;
            float stress = 0;
            if (_musicCurrent.MusicInstance.isValid())
            {
                _musicCurrent.MusicInstance.getParameterByName("Stress", out stress);
                if (!isSameMusic)
                {
                    crossFade = _currentVolume > 0 ? _currentVolume : 1;
                    while (crossFade > 0)
                    {
                        crossFade -= Time.deltaTime / _musicCurrent.Fade.Max;//Max is Fade Out duration
                        if (crossFade < 0) _currentVolume = 0f;
                        else _currentVolume = crossFade * SaveSystem.Instance.Options.musicVol;
                        _musicCurrent.MusicInstance.setVolume(_currentVolume);

                        yield return null;
                    }
                    _musicCurrent.MusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    _musicCurrent.MusicInstance.release();
                }
            }

            _musicCurrent = new MusicData(!isSameMusic ? RuntimeManager.CreateInstance(musicEventRef) : _musicCurrent.MusicInstance,
                        useDefaultFade ? new Range(_fadeInDuration, _fadeOutDuration) : fade,
                        useDefaultReplay ? _defaultMusicReplayDelayRange : replay,
                        useDefaultStartDelay ? _defaultMusicPlayDelayRange : startDelay);
            _musicCurrent.MusicInstance.setParameterByName("Stress", stress);
            yield return new WaitForSeconds(Random.Range(_musicCurrent.StartDelay.Min, _musicCurrent.StartDelay.Max));

            _musicCurrent.MusicInstance.setVolume(_currentVolume);
            if (!isSameMusic) _musicCurrent.MusicInstance.start();

            crossFade = _currentVolume > 0 ? _currentVolume : 0;
            while (crossFade < 1)
            {
                crossFade += Time.deltaTime / _musicCurrent.Fade.Min;//Min is FadeInDuration
                if (crossFade > 1) _currentVolume = 1f;
                else _currentVolume = crossFade * SaveSystem.Instance.Options.musicVol;
                _musicCurrent.MusicInstance.setVolume(_currentVolume);

                yield return null;
            }

            _transitionCoroutine = null;
            if (shouldStopPeriodically) _silenceRoutine = StartCoroutine(RandomlyMuteMusic());
        }

        private IEnumerator RandomlyMuteMusic()
        {
            float fade;
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(_musicCurrent.Replay.Min, _musicCurrent.Replay.Max));

                fade = _currentVolume > 0 ? _currentVolume : 1;
                while (fade > 0)
                {
                    fade -= Time.deltaTime / _musicCurrent.Fade.Max;//Max is Fade Out
                    if (fade < 0) _currentVolume = 0f;
                    else _currentVolume = fade * SaveSystem.Instance.Options.musicVol;
                    _musicCurrent.MusicInstance.setVolume(_currentVolume);

                    yield return null;
                }

                yield return new WaitForSeconds(Random.Range(_musicCurrent.StartDelay.Min, _musicCurrent.StartDelay.Max));

                fade = 0;
                while (fade < 1)
                {
                    fade += Time.deltaTime / _musicCurrent.Fade.Min;
                    if (fade > 1) _currentVolume = 1f;
                    else _currentVolume = fade * SaveSystem.Instance.Options.musicVol;
                    _musicCurrent.MusicInstance.setVolume(_currentVolume);

                    yield return null;
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_defaultMusicPlayDelayRange.Min > _defaultMusicPlayDelayRange.Max) _defaultMusicPlayDelayRange.Min = _defaultMusicPlayDelayRange.Max;
            if (_defaultMusicReplayDelayRange.Min > _defaultMusicReplayDelayRange.Max) _defaultMusicReplayDelayRange.Min = _defaultMusicReplayDelayRange.Max;
        }
#endif
    }
}