using UnityEngine;
using FMODUnity;

namespace Ivayami.Audio
{
    public class MusicTrigger : MonoBehaviour
    {
        [SerializeField] private EventReference _music;
        [SerializeField] private bool _forceStopCurrentMusicOnEnter;
        [SerializeField] private bool _useDefaultFade;
        [SerializeField] private Music.Range _fadeDuration;
        [SerializeField] private bool _useDefaultStartDelay;
        [SerializeField] private Music.Range _startDelay;
        [SerializeField] private bool _shouldStopPeriodicaly;
        [SerializeField] private bool _useDefaultReplay;
        [SerializeField] private Music.Range _replay;

        private void OnTriggerEnter(Collider other)
        {
            if (!Music.Instance) return;
            if (_forceStopCurrentMusicOnEnter) Music.Instance.ForceStop();
            Music.Instance.SetMusic(_music, _shouldStopPeriodicaly, _useDefaultFade, _useDefaultReplay, _useDefaultStartDelay, _fadeDuration, _replay, _startDelay);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!Music.Instance) return;
            Music.Instance.Stop(_music.Guid);
        }

        private void OnDisable()
        {
            if (!Music.Instance) return;
            Music.Instance.Stop(_music.Guid);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_startDelay.Min > _startDelay.Max) _startDelay.Min = _startDelay.Max;
            if (_replay.Min > _replay.Max) _replay.Min = _replay.Max;
        }
#endif
    }
}

