using UnityEngine;
using Ivayami.Misc;

namespace Ivayami.Scene
{
    public class ChangeFogClockTowerEvent : ClockTowerEvent
    {        
        [SerializeField] private InterpolateFogShader _startEventFogValue;
        [SerializeField] private InterpolateFogShader _endEventFogValue;
        [SerializeField] private InterpolateFogShader _interuptedEventFogValue;
        public override void StartEvent(float duration, bool debugLogs)
        {
            _startEventFogValue.StartLerp();
            base.StartEvent(duration + _startEventFogValue.LerpDuration, debugLogs);
        }

        protected override void StopEvent()
        {
            _endEventFogValue.StartLerp();
            Invoke(nameof(EndDelay), _endEventFogValue.LerpDuration);
        }       

        private void EndDelay()
        {
            base.StopEvent();
        }

        public override void InterruptEvent()
        {
            _startEventFogValue.StopLerp();
            _endEventFogValue.StopLerp();
            _interuptedEventFogValue.StartLerp();
            base.InterruptEvent();
        }
    }
}