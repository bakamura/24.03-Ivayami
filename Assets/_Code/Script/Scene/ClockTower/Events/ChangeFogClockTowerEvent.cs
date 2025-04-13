using UnityEngine;
using Ivayami.Puzzle;

namespace Ivayami.Scene
{
    public class ChangeFogClockTowerEvent : ClockTowerEvent
    {        
        [SerializeField] private InterpolateFogShader _startEventFogValue;
        [SerializeField] private InterpolateFogShader _endEventFogValue;
        [SerializeField] private InterpolateFogShader _interuptedEventFogValue;
        public override void StartEvent(float duration)
        {
            _startEventFogValue.StartLerp();
            base.StartEvent(duration + _startEventFogValue.LerpDuration);
        }

        public override void StopEvent()
        {
            _endEventFogValue.StartLerp();
            Invoke(nameof(base.StopEvent), _endEventFogValue.LerpDuration);
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