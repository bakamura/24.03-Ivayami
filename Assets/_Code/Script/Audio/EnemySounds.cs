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
        [SerializeField] private bool _autoActivateOnEnable;
        [SerializeField] private SphereCollider _activationArea;

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
            [Tooltip("The audio can have multiples of itself playing or play with other audio at the same time")] public bool CanPlayMultipleTimes;
            [Tooltip("If checked the audio will stop all audios currently playing that have this option active")] public bool CanBeStoped;
            public bool ReplayAudioOnEnd;
            public Range ReplayIntervalRange;
            [NonSerialized] public EventInstance AudioInstance;
            [NonSerialized] public Coroutine DelayToReplayCoroutine;
            [NonSerialized] public AudioCallbackData CallbackData;
            [NonSerialized] public bool WaitingForReplay;
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
            Steps,
            Attack
        }

        //public void PlaySound(int soundType) => PlaySound((SoundTypes)soundType, null);

        public void PlaySound(SoundTypes soundType, Action OnAudioEnd = null)
        {
            if (!_isActive) return;
            PLAYBACK_STATE state = PLAYBACK_STATE.STOPPED;
            GetValidSoundEventInArray(_audiosData, soundType, out SoundEventData newSound);
            if (newSound == null) return;
            if (newSound.CanBeStoped && !newSound.CanPlayMultipleTimes)
            {
                for (int i = 0; i < _currentSoundData.Count; i++)
                {
                    if (_currentSoundData[i].CanBeStoped && _currentSoundData[i].SoundType != newSound.SoundType && !_currentSoundData[i].CanPlayMultipleTimes)
                    {
                        if (_debugLog) UnityEngine.Debug.Log($"Stopping Enemy sound {_currentSoundData[i].SoundType} to play sound {soundType}");
                        if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING) _currentSoundData[i].AudioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                        PlayOnAudioEndEventCallback(i, true);
                    }
                }
            }

            SoundEventData currentSound = null;
            GetValidSoundEventInList(_currentSoundData, soundType, out currentSound);
            if (currentSound == null && (newSound.CanPlayMultipleTimes || !IsCurrentlyPlaying(_currentSoundData, soundType)))
            {
                TimelineInfo info = new TimelineInfo();
                AudioCallbackData callback = new AudioCallbackData(GCHandle.Alloc(info), info, new EVENT_CALLBACK(HandleOnAudioEnd), OnAudioEnd);
                currentSound = new SoundEventData(newSound, callback);
                currentSound.AudioInstance = InstantiateEvent(currentSound.AudioReference);
                if (_debugLog)
                    UnityEngine.Debug.Log($"New Enemy Sound EventInstance added for sound type {soundType}");               
                currentSound.AudioInstance.setUserData(GCHandle.ToIntPtr(currentSound.CallbackData.TimelineHandle));
                _currentSoundData.Add(currentSound);
            }
            if (currentSound != null && !currentSound.WaitingForReplay)
            {
                currentSound.WaitingForReplay = currentSound.ReplayAudioOnEnd;
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
            PLAYBACK_STATE state;
            for (int i = 0; i < array.Count; i++)
            {
                if (array[i].SoundType == type)
                {
                    array[i].AudioInstance.getPlaybackState(out state);
                    if (state == PLAYBACK_STATE.STOPPED)
                    {
                        data = array[i];
                        break;
                    }
                }
            }
        }

        private bool IsCurrentlyPlaying(List<SoundEventData> array, SoundTypes type)
        {
            PLAYBACK_STATE state;
            for (int i = 0; i < array.Count; i++)
            {
                if (array[i].SoundType == type)
                {
                    array[i].AudioInstance.getPlaybackState(out state);
                    if (state != PLAYBACK_STATE.STOPPED)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void GetValidSoundEventInArray(SoundEventData[] array, SoundTypes type, out SoundEventData data)
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

        private void ReleaseAllEvents()
        {
            PLAYBACK_STATE state;
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
                        if (PlayOnAudioEndEventCallback(i, false))
                        {
                            if (_debugLog)
                                UnityEngine.Debug.Log($"Audio End Replay Audio {_currentSoundData[i].SoundType}");
                            _currentSoundData[i].DelayToReplayCoroutine = StartCoroutine(ReplayDelayCoroutine(_currentSoundData[i]));
                        }
                    }
                }
                yield return delay;
            }
        }

        private IEnumerator ReplayDelayCoroutine(SoundEventData data)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(data.ReplayIntervalRange.Min, data.ReplayIntervalRange.Max));
            data.DelayToReplayCoroutine = null;
            data.WaitingForReplay = false;
            PlaySound(data.SoundType);
        }
        /// <summary>
        /// This method will play the event that was added in the OnAuidoEnd at PlaySound, if itis not in Replay will be removed from list. Returns true if the sound was removed
        /// </summary>
        /// <param name="eventIndex"></param>
        /// <returns></returns>
        private bool PlayOnAudioEndEventCallback(int eventIndex, bool forceRemoveFromList)
        {
            SoundEventData data = _currentSoundData[eventIndex];
            bool replayAudio = false;
            if (data.ReplayAudioOnEnd)
            {
                if (data.DelayToReplayCoroutine != null)
                {
                    StopCoroutine(data.DelayToReplayCoroutine);
                    data.DelayToReplayCoroutine = null;
                }
                replayAudio = true;
            }
            if (!data.ReplayAudioOnEnd && !data.CanPlayMultipleTimes || forceRemoveFromList)
            {
                _currentSoundData[eventIndex].AudioInstance.release();
                if(_debugLog) UnityEngine.Debug.Log($"Release Sound {_currentSoundData[eventIndex].SoundType}");
                _currentSoundData.RemoveAt(eventIndex);
            }
            if (_debugLog && data.CallbackData.UnityCallback != null) UnityEngine.Debug.Log($"Audio Callback End {data.SoundType}");
            data.CallbackData.UnityCallback?.Invoke();
            return replayAudio;
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

        private void OnEnable()
        {
            if (_autoActivateOnEnable) Activate();
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
                _activationArea.radius = 0;
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