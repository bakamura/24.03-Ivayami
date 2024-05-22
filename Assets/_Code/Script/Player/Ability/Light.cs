using UnityEngine;
using Ivayami.Puzzle;
using UnityEngine.Events;

namespace Ivayami.Player.Ability {
    public class Light : PlayerAbility {

        public UnityEvent onLightEnabled = new UnityEvent();
        public UnityEvent onLightDisabled = new UnityEvent();

        [SerializeField] private float _lightRadius;
        [SerializeField] private float _lightDistance;
        [SerializeField] private LayerMask _lightableLayer;
        private bool _enabled = false;

        private void Update() {
            if (_enabled) Illuminate();
        }

        public override void AbilityStart() {
            _enabled = !_enabled;

            Logger.Log(LogType.Player, $"Light {(_enabled ? "enabled" : "disabled")}");
        }

        public override void AbilityEnd() { }

        private void Illuminate() {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position + (transform.forward * _lightRadius), _lightRadius, transform.position + transform.forward, _lightDistance - _lightRadius, _lightableLayer);
            foreach (RaycastHit hit in hits) hit.transform.GetComponent<ILightable>()?.Iluminated();
        }

    }
}