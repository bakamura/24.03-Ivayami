using UnityEngine;
using Ivayami.Audio;

namespace Ivayami.Scene
{
    public class RainClockTowerEvent : ClockTowerEvent
    {
        [SerializeField] private ParticleSystem _rainParticles;
        [SerializeField] private float _endEventDelay;
        private SoundEffectTrigger _sound
        {
            get
            {
                if (!m_sound) m_sound = GetComponent<SoundEffectTrigger>();
                return m_sound;
            }
        }
        private SoundEffectTrigger m_sound;
        public override void StartEvent(float duration, bool debugLogs)
        {
            _sound.Play();
            _rainParticles.Play();
            base.StartEvent(duration, debugLogs);
        }

        protected override void StopEvent()
        {
            _sound.Stop();
            _rainParticles.Stop();
            Invoke(nameof(EndDelay), _endEventDelay);
        }

        private void EndDelay()
        {
            Destroy(_rainParticles.gameObject);
            base.StopEvent();
        }

        public override void InterruptEvent()
        {
            Destroy(_rainParticles.gameObject);
            base.InterruptEvent();
        }
    }
}