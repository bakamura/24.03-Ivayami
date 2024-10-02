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
        private SoundEventData _currentSoundData;
        private GCHandle _timelineHandle;
        private TimelineInfo _timelineInfo = new TimelineInfo();
        private EVENT_CALLBACK _audioEndCallback;
        private Coroutine _delayToReplayCoroutine;
        private Action _onAudioEnd;
        private bool _soundInterrupted;

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
                if(_soundInterrupted)
                {
                    StopReplayCoroutine();
                    _soundInterrupted = false;
                    return;
                }                    

                if (_currentSoundData.ReplayAudioOnEnd)
                {
                    if (_debugLog) UnityEngine.Debug.Log($"Audio End Replay Audio {_currentSoundData.SoundType}");
                    StopReplayCoroutine();
                    _delayToReplayCoroutine = StartCoroutine(ReplayDelayCoroutine());
                }

                if (_debugLog && _onAudioEnd != null) UnityEngine.Debug.Log($"Audio Callback End {_currentSoundData.SoundType}");
                _onAudioEnd?.Invoke();
                _onAudioEnd = null;
            }
        }

        public void PlaySound(SoundTypes soundType, bool bypassSameSoundCheck, Action OnAudioEnd = null)
        {
            if (!bypassSameSoundCheck && _currentSoundData.SoundType == soundType && _currentSoundData.AudioInstance.isValid())
            {
                return;
            }
            Setup();
            _currentSoundData.AudioInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING)
            {
                if (_debugLog) UnityEngine.Debug.Log($"Stopping sound {_currentSoundData.SoundType} to play sound {soundType}");
                _soundInterrupted = true;
                _currentSoundData.AudioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }

            for (int i = 0; i < _audiosData.Length; i++)
            {
                if (soundType == _audiosData[i].SoundType)
                {
                    _currentSoundData = _audiosData[i];
                    if (_debugLog) UnityEngine.Debug.Log($"PlaySound {soundType}");
                    break;
                }
            }
            if (_currentSoundData.ReplayAudioOnEnd || OnAudioEnd != null)
            {
                //UnityEngine.Debug.Log($"Audio {_currentSoundData.SoundType} has extra callback {OnAudioEnd != null}");
                _onAudioEnd = OnAudioEnd;
                _audioEndCallback = new EVENT_CALLBACK(HandleOnAudioEnd);
                _timelineHandle = GCHandle.Alloc(_timelineInfo);
                _currentSoundData.AudioInstance.setUserData(GCHandle.ToIntPtr(_timelineHandle));
                PlayOneShot(_currentSoundData.AudioInstance, false, _currentSoundData.AttenuationRange, _audioEndCallback);
            }
            else PlayOneShot(_currentSoundData.AudioInstance, false, _currentSoundData.AttenuationRange);
        }

        private void Setup()
        {
            if (!_hasDoneSetup)
            {
                //EventInstance instance;
                for (int i = 0; i < _audiosData.Length; i++)
                {
                    if (!_audiosData[i].AudioReference.IsNull)
                    {
                        //if (ContainsEventInstance(_audiosData[i].AudioReference, out instance))
                        //{
                        //    _audiosData[i].AudioInstance = instance;
                        //}
                        //else
                        //{
                            _audiosData[i].AudioInstance = InstantiateEvent(_audiosData[i].AudioReference);
                        //}
                    }
                }
                _hasDoneSetup = true;
            }
        }

        //private bool ContainsEventInstance(EventReference eventReference, out EventInstance instance)
        //{
        //    for(int i = 0; i < _audiosData.Length; i++)
        //    {
        //        if(eventReference.Equals(_audiosData[i].AudioReference) && _audiosData[i].AudioInstance.isValid())
        //        {
        //            print($"Found instance of {eventReference.Path}");
        //            instance = _audiosData[i].AudioInstance; 
        //            return true;
        //        }
        //    }
        //    print($"No Reference Found instance of {eventReference.Path}, create instance");
        //    instance = new EventInstance();
        //    return false;
        //}

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
            yield return new WaitForSeconds(UnityEngine.Random.Range(_currentSoundData.ReplayIntervalRange.Min, _currentSoundData.ReplayIntervalRange.Max));
            _delayToReplayCoroutine = null;
            PlaySound(_currentSoundData.SoundType, true);
        }

        private static RESULT HandleOnAudioEnd(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
        {
            EventInstance instance = new EventInstance(instancePtr);
            //instance.getDescription(out EventDescription descrip);
            //descrip.getPath(out string path);
            //print($"Callback from {path}");
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