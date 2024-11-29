using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using FMOD;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using AOT;

namespace Ivayami.Audio
{
    public class EnemySounds : EntitySound
    {
        [SerializeField] private SoundEventData[] _audiosData;
        [SerializeField] private bool _debugLog;
        [SerializeField] private SphereCollider _activationArea;

        private bool _hasDoneSetup;
        private bool _isActive;
        private const float _updateTick = .2f;
        private List<SoundEventData> _currentSoundData = new List<SoundEventData>();
        private Coroutine _updateCoroutine;

        [Serializable]
        private class SoundEventData
        {
            public SoundTypes SoundType;
            public EventReference AudioReference;
            public Range AttenuationRange;
            public bool CanPlayMultipleTimes;
            [Tooltip("If checked the audio will stop playing whenever a new sound starts that is marked with this option")] public bool CanBeStoped;
            public bool ReplayAudioOnEnd;
            public Range ReplayIntervalRange;
            [HideInInspector] public EventInstance AudioInstance;
            [HideInInspector] public Coroutine DelayToReplayCoroutine;
            [HideInInspector] public AudioCallbackData CallbackData;
#if UNITY_EDITOR
            [Tooltip("Can draw only with 3D sounds")] public bool DrawGizmos;
            public Color MinRangGizmoColor;
            public Color MaxRangGizmoColor;
#endif
            public SoundEventData(SoundEventData data, AudioCallbackData callback)
            {
                SoundType = data.SoundType;
                AudioReference = data.AudioReference;
                AttenuationRange = data.AttenuationRange;
                CanPlayMultipleTimes = data.CanPlayMultipleTimes;
                CanBeStoped = data.CanBeStoped;
                ReplayAudioOnEnd = data.ReplayAudioOnEnd;
                ReplayIntervalRange = data.ReplayIntervalRange;
                AudioInstance = data.AudioInstance;
                DelayToReplayCoroutine = data.DelayToReplayCoroutine;
                CallbackData = callback;
            }
        }

        private class AudioCallbackData
        {
            public GCHandle TimelineHandle;
            public TimelineInfo TimelineInfo;
            public EVENT_CALLBACK FMODCallback;
            public Action UnityCallback;

            public AudioCallbackData(GCHandle handle, TimelineInfo info, EVENT_CALLBACK fmodCallback, Action unityCallback)
            {
                TimelineHandle = handle;
                TimelineInfo = info;
                FMODCallback = fmodCallback;
                UnityCallback = unityCallback;
            }
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
            Chasing,
            Steps
        }

        public void PlaySound(SoundTypes soundType, Action OnAudioEnd = null)
        {
            if (!_isActive) return;
            Setup();
            PLAYBACK_STATE state = PLAYBACK_STATE.STOPPED;
            GetValidSoundEventInList(_currentSoundData, soundType, out SoundEventData currentSound);
            GetValidSoundEventInList(_audiosData, soundType, out SoundEventData newSound);
            if (currentSound == null && newSound.CanBeStoped && !newSound.CanPlayMultipleTimes)/*(data == null || (data != null && data.SoundType != soundType)*/
            {
                for (int i = 0; i < _currentSoundData.Count; i++)
                {
                    _currentSoundData[i].AudioInstance.getPlaybackState(out state);
                    if (_currentSoundData[i].CanBeStoped)
                    {
                        if (_debugLog) UnityEngine.Debug.Log($"Stopping Enemy sound {_currentSoundData[i].SoundType} to play sound {soundType}");
                        if (state == PLAYBACK_STATE.PLAYING) _currentSoundData[i].AudioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                        PlayAudioEventCallback(i, true);
                    }
                }
            }

            bool willPlaySound = false;
            for (int i = 0; i < _audiosData.Length; i++)
            {
                if (soundType == _audiosData[i].SoundType)
                {
                    GetValidSoundEventInList(_currentSoundData, soundType, out currentSound);
                    if (currentSound != null) currentSound.AudioInstance.getPlaybackState(out state);
                    if (currentSound == null || (currentSound != null && state == PLAYBACK_STATE.PLAYING && currentSound.CanPlayMultipleTimes))
                    {
                        TimelineInfo info = new TimelineInfo();
                        AudioCallbackData callback = new AudioCallbackData(GCHandle.Alloc(info), info, new EVENT_CALLBACK(HandleOnAudioEnd), OnAudioEnd);
                        currentSound = new SoundEventData(_audiosData[i], callback);
                        if (currentSound.CanPlayMultipleTimes)
                        {
                            currentSound.AudioInstance = InstantiateEvent(currentSound.AudioReference);
                            if (_debugLog) UnityEngine.Debug.Log($"New Enemy Sound EventInstance added for sound type {soundType}");
                        }
                        currentSound.AudioInstance.setUserData(GCHandle.ToIntPtr(currentSound.CallbackData.TimelineHandle));
                        _currentSoundData.Add(currentSound);
                        willPlaySound = true;
                    }
                    else if (currentSound != null && currentSound.ReplayAudioOnEnd && state == PLAYBACK_STATE.STOPPED)
                    {
                        willPlaySound = true;
                    }
                    break;
                }
            }
            if (willPlaySound)
            {
                PlayOneShot(currentSound.AudioInstance, false, currentSound.AttenuationRange, currentSound.CallbackData.FMODCallback);
                if (_debugLog) UnityEngine.Debug.Log($"PlayEnemySound {soundType}");
            }
        }

        public void Activate()
        {
            _isActive = true;
            if (_updateCoroutine == null)
            {
                _updateCoroutine = StartCoroutine(UpdateCoroutine());
            }
        }

        public void Deactivate()
        {
            _isActive = false;
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
            ReleaseAllEvents();
        }

        private void GetValidSoundEventInList(List<SoundEventData> array, SoundTypes type, out SoundEventData data)
        {
            data = null;
            for (int i = 0; i < array.Count; i++)
            {
                if (array[i].SoundType == type)
                {
                    data = array[i];
                    break;
                }
            }
        }

        private void GetValidSoundEventInList(SoundEventData[] array, SoundTypes type, out SoundEventData data)
        {
            data = null;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].SoundType == type)
                {
                    data = array[i];
                    break;
                }
            }
        }

        private void Setup()
        {
            if (!_hasDoneSetup)
            {
                for (int i = 0; i < _audiosData.Length; i++)
                {
                    if (!_audiosData[i].AudioReference.IsNull)
                    {
                        _audiosData[i].AudioInstance = InstantiateEvent(_audiosData[i].AudioReference);
                    }
                }
                _hasDoneSetup = true;
            }
        }

        private void ReleaseAllEvents()
        {
            PLAYBACK_STATE state;
            if (_audiosData != null)
            {
                for (int i = 0; i < _audiosData.Length; i++)
                {
                    if (!_audiosData[i].AudioInstance.isValid()) return;
                    _audiosData[i].AudioInstance.getPlaybackState(out state);
                    if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING) _audiosData[i].AudioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    _audiosData[i].AudioInstance.release();
                }
            }
            if (_currentSoundData != null)
            {
                for (int i = 0; i < _currentSoundData.Count; i++)
                {
                    if (!_currentSoundData[i].AudioInstance.isValid()) return;
                    _currentSoundData[i].AudioInstance.getPlaybackState(out state);
                    if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING) _currentSoundData[i].AudioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    _currentSoundData[i].AudioInstance.release();
                }
            }
            _hasDoneSetup = false;
        }

        private IEnumerator ReplayDelayCoroutine(SoundTypes type, Range range)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(range.Min, range.Max));
            PlaySound(type);
        }

        private IEnumerator UpdateCoroutine()
        {
            WaitForSeconds delay = new WaitForSeconds(_updateTick);
            while (true)
            {
                if (_currentSoundData == null) yield return null;
                for (int i = 0; i < _currentSoundData.Count; i++)
                {
                    if (_currentSoundData[i].CallbackData.TimelineInfo.HasEnded)
                    {
                        _currentSoundData[i].CallbackData.TimelineInfo.HasEnded = false;
                        if (!PlayAudioEventCallback(i, false))
                        {
                            if (_debugLog) UnityEngine.Debug.Log($"Audio End Replay Audio {_currentSoundData[i].SoundType}");
                            _currentSoundData[i].DelayToReplayCoroutine =
                                StartCoroutine(ReplayDelayCoroutine(_currentSoundData[i].SoundType, _currentSoundData[i].ReplayIntervalRange));
                        }
                    }
                }
                yield return delay;
            }
        }
        /// <summary>
        /// Returns True if has removed the event from the list
        /// </summary>
        /// <param name="eventIndex"></param>
        /// <returns></returns>
        private bool PlayAudioEventCallback(int eventIndex, bool forceRemoveFromList)
        {
            SoundEventData data = _currentSoundData[eventIndex];
            bool result = false;
            if (data.ReplayAudioOnEnd && data.DelayToReplayCoroutine != null)
            {
                StopCoroutine(data.DelayToReplayCoroutine);
                data.DelayToReplayCoroutine = null;
            }
            if (!data.ReplayAudioOnEnd || forceRemoveFromList)
            {
                _currentSoundData.RemoveAt(eventIndex);
                result = true;
            }
            if (_debugLog && data.CallbackData.UnityCallback != null) UnityEngine.Debug.Log($"Audio Callback End {data.SoundType}");
            data.CallbackData.UnityCallback?.Invoke();
            return result;
        }
        [MonoPInvokeCallback(typeof(RESULT))]
        private static RESULT HandleOnAudioEnd(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
        {
            EventInstance instance = new EventInstance(instancePtr);
            RESULT result = instance.getUserData(out IntPtr timelineInfoPtr);
            if (result != RESULT.OK)
            {
                print("Timeline Callback error: " + result);
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
            ReleaseAllEvents();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            int lenght = Enum.GetNames(typeof(SoundTypes)).Length;
            if (_audiosData != null && _audiosData.Length > lenght)
            {
                Array.Resize(ref _audiosData, lenght);
            }
            for (int i = 0; i < _audiosData.Length; i++)
            {
                if (_audiosData[i].CanBeStoped && _audiosData[i].CanPlayMultipleTimes) _audiosData[i].CanPlayMultipleTimes = false;
            }
            if (_activationArea)
            {
                for (int i = 0; i < _audiosData.Length; i++)
                {
                    if (_audiosData[i].AttenuationRange.Max > _activationArea.radius) _activationArea.radius = _audiosData[i].AttenuationRange.Max;
                }
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