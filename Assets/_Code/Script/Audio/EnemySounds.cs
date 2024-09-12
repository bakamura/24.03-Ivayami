using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using FMOD;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Ivayami.Audio
{
    public class EnemySounds : EntitySound
    {
        [SerializeField] private SoundEventData[] _audiosData;
        [SerializeField] private bool _debugLog;

        private bool _hasDoneSetup;
        private SoundEventData _currentSounData;
        private GCHandle _timelineHandle;
        private TimelineInfo _timelineInfo = new TimelineInfo();
        private EVENT_CALLBACK _audioEndCallback;
        private Coroutine _delayToReplayCoroutine;
        private Action _onAudioEnd;

        [Serializable]
        private struct SoundEventData
        {
            public SoundTypes SoundType;
            public EventReference AudioReference;
            public Range AttenuationRange;
            public bool ReplayAudioOnEnd;
            public Range ReplayIntervalRange;
            [HideInInspector] public EventInstance AudioInstance;
#if UNITY_EDITOR
            public bool DrawGizmos;
            public Color MinRangGizmoColor;
            public Color MaxRangGizmoColor;
#endif
        }

        private class TimelineInfo
        {
            public bool HasEnded;
        }

        public enum SoundTypes
        {
            TargetDetected,
            TakeDamage,
            IdleScreams,
            Chasing
        }

        private void Update()
        {
            if (_timelineInfo.HasEnded)
            {
                _timelineInfo.HasEnded = false;
                _onAudioEnd?.Invoke();
                _onAudioEnd = null;
                if (_currentSounData.ReplayAudioOnEnd)
                {
                    StopReplayCoroutine();
                    _delayToReplayCoroutine = StartCoroutine(ReplayDelayCoroutine());
                }
            }
        }

        public void PlaySound(SoundTypes soundType, Action OnAudioEnd = null)
        {
            if (_currentSounData.SoundType == soundType && _currentSounData.AudioInstance.isValid()) return;
            Setup();
            _currentSounData.AudioInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING)
            {
                if (_debugLog) UnityEngine.Debug.Log($"Stopping sound {_currentSounData.SoundType} to play sound {soundType}");
                _currentSounData.AudioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }

            for (int i = 0; i < _audiosData.Length; i++)
            {
                if (soundType == _audiosData[i].SoundType)
                {
                    _currentSounData = _audiosData[i];
                    if (_debugLog) UnityEngine.Debug.Log($"PlaySound {soundType}");
                    break;
                }
            }            
            if (_currentSounData.ReplayAudioOnEnd || OnAudioEnd != null)
            {
                _onAudioEnd = OnAudioEnd;
                _audioEndCallback = new EVENT_CALLBACK(HandleOnAudioEnd);
                _timelineHandle = GCHandle.Alloc(_timelineInfo);
                _currentSounData.AudioInstance.setUserData(GCHandle.ToIntPtr(_timelineHandle));
                PlayOneShot(_currentSounData.AudioInstance, false, _currentSounData.AttenuationRange, _audioEndCallback);
            }
            else PlayOneShot(_currentSounData.AudioInstance, false, _currentSounData.AttenuationRange);
        }

        private void Setup()
        {
            if (!_hasDoneSetup)
            {
                for (int i = 0; i < _audiosData.Length; i++)
                {
                    if (!_audiosData[i].AudioReference.IsNull) _audiosData[i].AudioInstance = InstantiateEvent(_audiosData[i].AudioReference);
                }
                _hasDoneSetup = true;
            }
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
            yield return new WaitForSeconds(UnityEngine.Random.Range(_currentSounData.ReplayIntervalRange.Min, _currentSounData.ReplayIntervalRange.Max));
            _delayToReplayCoroutine = null;
            PlaySound(_currentSounData.SoundType);
        }

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

        private void OnDisable()
        {
            for (int i = 0; i < _audiosData.Length; i++)
            {
                if (_audiosData[i].AudioInstance.isValid()) _audiosData[i].AudioInstance.release();
            }
            _hasDoneSetup = false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            int lenght = Enum.GetNames(typeof(SoundTypes)).Length;
            if (_audiosData != null && _audiosData.Length > lenght)
            {
                Array.Resize(ref _audiosData, lenght);
            }
        }

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