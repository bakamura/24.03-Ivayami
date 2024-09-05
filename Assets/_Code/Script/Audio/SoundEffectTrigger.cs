using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using FMOD;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Ivayami.Audio
{
    public class SoundEffectTrigger : EntitySound
    {
        [SerializeField] private EventReference _audioToPlay;
        [SerializeField] private bool _playOnStart;
        [SerializeField] private bool _replayAudioOnEnd;
        [SerializeField] private Range _replayIntervalRange;

        [Serializable]
        private struct Range
        {
            [Min(0f)] public float Min;
            [Min(0f)] public float Max;
        }

        private EventInstance _soundInstance;
        private GCHandle _timelineHandle;
        private TimelineInfo _timelineInfo = new TimelineInfo();
        private EVENT_CALLBACK _audioEndCallback;
        private Coroutine _delayToReplayCoroutine;
        private bool _hasDoneSetup;

        private class TimelineInfo
        {
            public bool HasEnded;
        }

        private void Start()
        {
            if (_playOnStart) Play();
        }

        private void Update()
        {
            if (_timelineInfo.HasEnded && _replayAudioOnEnd)
            {
                _timelineInfo.HasEnded = false;
                StopReplayCoroutine();
                _delayToReplayCoroutine = StartCoroutine(ReplayDelayCoroutine());
            }
        }

        #region Behaviour
        [ContextMenu("Play")]
        public void Play()
        {
            Setup();
            if (_replayAudioOnEnd)
            {
                _audioEndCallback = new EVENT_CALLBACK(HandleOnAudioEnd);
                _timelineHandle = GCHandle.Alloc(_timelineInfo);
                _soundInstance.setUserData(GCHandle.ToIntPtr(_timelineHandle));
                PlayOneShot(_soundInstance, _audioEndCallback);
            }
            else PlayOneShot(_soundInstance);
        }

        [ContextMenu("Pause")]
        public void Pause()
        {
            _soundInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STOPPED)
            {
                _soundInstance.getPaused(out bool paused);
                _soundInstance.setPaused(!paused);
            }
        }
        [ContextMenu("Stop")]
        public void Stop()
        {
            _soundInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING) _soundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }

        private void Setup()
        {
            if (!_hasDoneSetup)
            {
                _soundInstance = InstantiateEvent(_audioToPlay);
                _hasDoneSetup = true;
            }
        }
        #endregion

        #region Callback
        private static RESULT HandleOnAudioEnd(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
        {
            EventInstance instance = new EventInstance(instancePtr);
            RESULT result = instance.getUserData(out IntPtr timelineInfoPtr);
            if (result != RESULT.OK)
            {
                UnityEngine.Debug.LogError("Timeline Callback error: " + result);
            }
            else if (timelineInfoPtr != IntPtr.Zero)
            {
                // Get the object to store data
                GCHandle timelineHandle = GCHandle.FromIntPtr(timelineInfoPtr);
                TimelineInfo timelineInfo = (TimelineInfo)timelineHandle.Target;

                timelineInfo.HasEnded = true;
            }
            return RESULT.OK;
        }

        private void StopReplayCoroutine()
        {
            if (_delayToReplayCoroutine != null)
            {
                StopCoroutine(_delayToReplayCoroutine);
                _delayToReplayCoroutine = null;
            }
        }

        private IEnumerator ReplayDelayCoroutine()
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(_replayIntervalRange.Min, _replayIntervalRange.Max));
            Play();
            _delayToReplayCoroutine = null;
        }
        #endregion

        private void OnDisable()
        {
            StopReplayCoroutine();
        }

        private void OnDestroy()
        {
            if (_soundInstance.isValid())
                _soundInstance.release();
        }
    }
}