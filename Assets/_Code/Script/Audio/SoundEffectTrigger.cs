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
        [SerializeField] private SoundEventData[] _audiosData;
        [SerializeField] private bool _playOnStart;
        [SerializeField] private bool _replayAudioOnEnd;
        [SerializeField] private Range _replayIntervalRange;

        [Serializable]
        private struct SoundEventData
        {
            public EventReference AudioReference;
            public bool AllowFadeOut;
            public Range AttenuationRange;
            [HideInInspector] public EventInstance AudioInstance;
#if UNITY_EDITOR
            [Tooltip("Will only show if the audio is 3D")] public bool DrawGizmos;
            public Color MinRangGizmoColor;
            public Color MaxRangGizmoColor;
            public SoundEventData(EventReference reference, bool allowFadeOut, Range attenuation, EventInstance instance, bool drawGizmos, Color minRange, Color maxRange)
            {
                AudioReference = reference;
                AllowFadeOut = allowFadeOut;
                AttenuationRange = attenuation;
                AudioInstance = instance;
                DrawGizmos = drawGizmos;
                MinRangGizmoColor = minRange;
                MaxRangGizmoColor = maxRange;
            }
#endif
        }

        private SoundEventData _currentSounData;
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
            _currentSounData = _audiosData[UnityEngine.Random.Range(0, _audiosData.Length - 1)];
            if (_replayAudioOnEnd)
            {
                _audioEndCallback = new EVENT_CALLBACK(HandleOnAudioEnd);
                _timelineHandle = GCHandle.Alloc(_timelineInfo);
                _currentSounData.AudioInstance.setUserData(GCHandle.ToIntPtr(_timelineHandle));
                PlayOneShot(_currentSounData.AudioInstance, _currentSounData.AllowFadeOut, _currentSounData.AttenuationRange, _audioEndCallback);
            }
            else PlayOneShot(_currentSounData.AudioInstance, _currentSounData.AllowFadeOut, _currentSounData.AttenuationRange);
        }

        [ContextMenu("Pause")]
        public void Pause()
        {
            _currentSounData.AudioInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STOPPED)
            {
                _currentSounData.AudioInstance.getPaused(out bool paused);
                _currentSounData.AudioInstance.setPaused(!paused);
            }
        }

        [ContextMenu("Stop")]
        public void Stop()
        {
            _currentSounData.AudioInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING) _currentSounData.AudioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }

        private void Setup()
        {
            if (!_hasDoneSetup)
            {
                for (int i = 0; i < _audiosData.Length; i++)
                {
                    _audiosData[i].AudioInstance = InstantiateEvent(_audiosData[i].AudioReference);
                }
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
            if (_audiosData == null) return;
            for (int i = 0; i < _audiosData.Length; i++)
            {
                if (_audiosData[i].AudioInstance.isValid()) _audiosData[i].AudioInstance.release();
            }
            _hasDoneSetup = false;
        }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_audiosData == null) return;
            EditorUtils.LoadPreviewBanks();
            EventDescription[] descriptions = new EventDescription[_audiosData.Length];
            int i;
            for (i = 0; i < _audiosData.Length; i++)
            {
                if (!_audiosData[i].AudioReference.IsNull) EditorUtils.System.getEventByID(_audiosData[i].AudioReference.Guid, out descriptions[i]);
            }
            float min;
            float max;
            bool is3D;
            for (i = 0; i < descriptions.Length; i++)
            {
                if (descriptions[i].isValid() && _audiosData[i].DrawGizmos)
                {
                    descriptions[i].is3D(out is3D);
                    if (!is3D) return;
                    descriptions[i].getMinMaxDistance(out min, out max);
                    Gizmos.color = _audiosData[i].MinRangGizmoColor;
                    Gizmos.DrawWireSphere(transform.position, _audiosData[i].AttenuationRange.Min > 0 ? _audiosData[i].AttenuationRange.Min : min);
                    Gizmos.color = _audiosData[i].MaxRangGizmoColor;
                    Gizmos.DrawWireSphere(transform.position, _audiosData[i].AttenuationRange.Max > 0 ? _audiosData[i].AttenuationRange.Max : max);
                }
            }
        }
#endif
    }
}