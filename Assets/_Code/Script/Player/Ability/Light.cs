using UnityEngine;
using Ivayami.Puzzle;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Ivayami.Player.Ability {
    public class Light : PlayerAbility {

        public UnityEvent onLightEnabled = new UnityEvent();
        public UnityEvent onLightDisabled = new UnityEvent();

        [SerializeField] private float _lightRadius;
        [SerializeField] private float _lightDistance;
        [SerializeField] private LayerMask _lightableLayer;
        private bool _enabled = false;

        private List<ILightable> _iluminatedObjects = new List<ILightable>();
        public static Vector3 IluminatedPoint { get; private set; }

        private void Update() {
            if (_enabled) Illuminate();
        }

        public override void AbilityStart() {
            _enabled = !_enabled;
            if(!_enabled) IluminatedPoint = Vector3.zero;

            Logger.Log(LogType.Player, $"Light {(_enabled ? "enabled" : "disabled")}");
        }

        public override void AbilityEnd() { }

        private void Illuminate() {
            IluminatedPoint = Physics.Raycast(transform.position, transform.forward, out RaycastHit hitLine) ? hitLine.point : Vector3.zero;
            RaycastHit[] hits = Physics.SphereCastAll(transform.position + (transform.forward * _lightRadius), _lightRadius, transform.position + transform.forward, _lightDistance - _lightRadius, _lightableLayer);
            Queue<ILightable> hitLightables = new Queue<ILightable>();
            foreach (RaycastHit hit in hits) hitLightables.Enqueue(hit.transform.GetComponent<ILightable>());
            List<ILightable> ligthablesToStop = _iluminatedObjects;
            ILightable iteratorLightable;
            while (hitLightables.Count > 0) {
                iteratorLightable = hitLightables.Dequeue();
                if (_iluminatedObjects.Contains(iteratorLightable)) ligthablesToStop.Remove(iteratorLightable);
                else {
                    iteratorLightable.Iluminate();
                    _iluminatedObjects.Add(iteratorLightable);
                }
            }
            foreach (ILightable lightable in ligthablesToStop) lightable.IluminateStop();
        }

    }
}