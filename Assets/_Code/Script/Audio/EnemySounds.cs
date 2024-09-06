using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Ivayami.Audio
{
    public class EnemySounds : EntitySound
    {
        [SerializeField] private EventData[] _audiosData;
        [SerializeField] private bool _debugLog;

        private bool _hasDoneSetup;

        [System.Serializable]
        private struct EventData
        {
            public SoundTypes SoundType;
            public EventReference AudioReference;
            public Range AttenuationRange;
            [HideInInspector] public EventInstance AudioInstance;
#if UNITY_EDITOR
            public bool DrawGizmos;
            public Color MinRangGizmoColor;
            public Color MaxRangGizmoColor;
#endif
        }

        public enum SoundTypes
        {
            TargetDetected,
            TakeDamage
        }

        public void PlaySound(SoundTypes soundType)
        {
            Setup();
            if (_debugLog) Debug.Log($"PlaySound {soundType}");
            for(int i = 0; i < _audiosData.Length; i++)
            {
                if (soundType == _audiosData[i].SoundType) PlayOneShot(_audiosData[i].AudioInstance, false, _audiosData[i].AttenuationRange);
            }
        }

        private void Setup()
        {
            if (!_hasDoneSetup)
            {
                for(int i =0; i < _audiosData.Length; i++)
                {
                    if(!_audiosData[i].AudioReference.IsNull) _audiosData[i].AudioInstance = InstantiateEvent(_audiosData[i].AudioReference);
                }
                _hasDoneSetup = true;
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _audiosData.Length; i++)
            {
                if (_audiosData[i].AudioInstance.isValid()) _audiosData[i].AudioInstance.release();
            }
        }
    }
}