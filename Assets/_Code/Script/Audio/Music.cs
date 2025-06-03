using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Ivayami.Player;
using Ivayami.Save;
using System.Collections.Generic;
using Ivayami.UI;

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

        private List<MusicData> _musicPlaylist = new List<MusicData>();
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
#if UNITY_EDITOR
        [System.Serializable]
#endif
        private class MusicData
        {
            public EventInstance MusicInstance;
#if UNITY_EDITOR
            public string Name;
#endif
            public Range Fade;
            public Range Replay;
            public Range StartDelay;
            public bool ShouldStopPeriodically;
            public bool RemoveFromPlaylistOnNewTransition;
            public FMOD.GUID Guid;

            public MusicData(EventInstance eventInstance, Range fade, Range replay, Range startDelay, bool shouldStopPeriodically, bool removeFromPlaylistOnNewTransition)
            {
                MusicInstance = eventInstance;
#if UNITY_EDITOR
                eventInstance.getDescription(out EventDescription name);
                name.getPath(out Name);
#endif
                Fade = fade;
                Replay = replay;
                StartDelay = startDelay;
                ShouldStopPeriodically = shouldStopPeriodically;
                RemoveFromPlaylistOnNewTransition = removeFromPlaylistOnNewTransition;
                eventInstance.getDescription(out EventDescription description);
                description.getID(out Guid);
            }
        }

        private void Start()
        {
            StressIndicatorSmoother.Instance.OnStressSmoothed.AddListener(UpdateMusicToStress);
        }

        public void SetMusic(EventReference musicEventRef, bool shouldStopPeriodically, bool removeFromPlaylistOnNewTransition, bool useDefaultFade, bool useDefaultReplay, bool useDefaultStartDelay, Range fade = new(), Range replay = new(), Range startDelay = new())
        {
            if (!musicEventRef.IsNull)
            {
                StopCoroutines();
                bool isSameMusic = _musicCurrent != null && _musicCurrent.Guid == musicEventRef.Guid;
                if (!isSameMusic)
                {
                    if (!CheckForMusicInPlaylist(musicEventRef.Guid, out int index))
                    {
                        _musicPlaylist.Add(new MusicData(RuntimeManager.CreateInstance(musicEventRef),
                                useDefaultFade ? new Range(_fadeInDuration, _fadeOutDuration) : fade,
                                useDefaultReplay ? _defaultMusicReplayDelayRange : replay,
                                useDefaultStartDelay ? _defaultMusicPlayDelayRange : startDelay,
                                shouldStopPeriodically, removeFromPlaylistOnNewTransition));
                    }
                    else
                    {
                        MusicData temp = new MusicData(_musicPlaylist[index].MusicInstance,
                            useDefaultFade ? new Range(_fadeInDuration, _fadeOutDuration) : fade,
                            useDefaultReplay ? _defaultMusicReplayDelayRange : replay,
                            useDefaultStartDelay ? _defaultMusicPlayDelayRange : startDelay,
                            shouldStopPeriodically, removeFromPlaylistOnNewTransition);
                        _musicPlaylist.RemoveAt(index);
                        _musicPlaylist.Insert(_musicPlaylist.Count, temp);
                    }
                }
                else
                {
                    _musicPlaylist[^1].Fade = useDefaultFade ? new Range(_fadeInDuration, _fadeOutDuration) : fade;
                    _musicPlaylist[^1].Replay = useDefaultReplay ? _defaultMusicReplayDelayRange : replay;
                    _musicPlaylist[^1].StartDelay = useDefaultStartDelay ? _defaultMusicPlayDelayRange : startDelay;
                    _musicPlaylist[^1].ShouldStopPeriodically = shouldStopPeriodically;
                    _musicPlaylist[^1].RemoveFromPlaylistOnNewTransition = removeFromPlaylistOnNewTransition;
                }
                _transitionCoroutine = StartCoroutine(TransitionMusic(_musicPlaylist[^1]));
            }
        }

        public void Stop(FMOD.GUID guid)
        {
            if (guid == _musicCurrent.Guid)
            {
                StopCoroutines();
                _fadeOutRoutine = StartCoroutine(StopRoutine(guid));
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

        private IEnumerator StopRoutine(FMOD.GUID guid)
        {
            if (_musicPlaylist.Count == 0 || !CheckForMusicInPlaylist(guid, out _)) yield break;
            if (_musicCurrent.Guid == guid)
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
                _musicPlaylist.Remove(_musicCurrent);
                _musicCurrent.MusicInstance.release();
                _musicCurrent = null;
            }
            else
            {
                for (int i = 0; i < _musicPlaylist.Count; i++)
                {
                    if (_musicPlaylist[i].Guid == guid)
                    {
                        _musicPlaylist[i].MusicInstance.release();
                        _musicPlaylist.RemoveAt(i);
                        break;
                    }
                }
            }
            _fadeOutRoutine = null;
            if (_musicPlaylist.Count > 0 && _musicCurrent == null)
            {
                _transitionCoroutine = StartCoroutine(TransitionMusic(_musicPlaylist[^1]));
            }
        }

        private bool CheckForMusicInPlaylist(FMOD.GUID guid, out int index)
        {
            index = 0;
            for (int i = 0; i < _musicPlaylist.Count; i++)
            {
                if (_musicPlaylist[i].Guid == guid)
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        public void ForceStop()
        {
            if (_musicPlaylist.Count == 0 || _musicCurrent == null) return;
            StopCoroutines();
            _musicCurrent.MusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _musicPlaylist.Remove(_musicCurrent);
            _musicCurrent.MusicInstance.release();
            _musicCurrent = null;
        }

        private void UpdateMusicToStress(float stress)
        {
            if (_musicCurrent != null) _musicCurrent.MusicInstance.setParameterByName("Stress", stress / PlayerStress.Instance.MaxStress);
        }

        private IEnumerator TransitionMusic(MusicData musicData)
        {
            float crossFade;
            bool isSameMusic = _musicCurrent != null && _musicCurrent.Guid == _musicPlaylist[^1].Guid;
            float stress = 0;
            if (_musicCurrent != null)
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
                    if (_musicCurrent.RemoveFromPlaylistOnNewTransition)
                    {
                        _musicPlaylist.Remove(_musicCurrent);
                        _musicCurrent.MusicInstance.release();
                    }
                }
            }

            Range startDelay = new Range(_musicPlaylist[^1].StartDelay.Min, _musicPlaylist[^1].StartDelay.Max);

            yield return new WaitForSeconds(Random.Range(startDelay.Min, startDelay.Max));

            _musicCurrent = _musicPlaylist[^1];
            _musicCurrent.MusicInstance.setParameterByName("Stress", stress);
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
            if (_musicCurrent.ShouldStopPeriodically) _silenceRoutine = StartCoroutine(RandomlyMuteMusic());
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