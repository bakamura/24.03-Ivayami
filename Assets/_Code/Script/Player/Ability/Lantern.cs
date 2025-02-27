using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Ivayami.Puzzle;

namespace Ivayami.Player.Ability {
    public class Lantern : PlayerAbility {

        public static UnityEvent<Vector3> OnIlluminate { get; private set; } = new UnityEvent<Vector3>();

        [SerializeField] private Transform _origin;
        [SerializeField] private float _lightRadius;
        [SerializeField] private float _lightFocusedRadius;
        [SerializeField] private float _lightDistance;
        [SerializeField] private LayerMask _lightableLayer;
        [SerializeField] private LayerMask _occlusionLayer;
        private bool _enabled = false;
        private bool _focused = false;
        [SerializeField] private int _lightMaxHitNumber;


        [Header("Cache")]

        private HashSet<ILightable> _illuminatedObjects = new HashSet<ILightable>();
        private HashSet<ILightable> _stopIlluminating = new HashSet<ILightable>();
        private Collider[] _lightHits;
        private float _coneAngleHalf;

        private MeshRenderer[] _meshRenderers;

        private void Awake() {
            _coneAngleHalf = GetComponentInChildren<Light>().spotAngle / 2f;
            _lightHits = new Collider[_lightMaxHitNumber];
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in _meshRenderers) meshRenderer.enabled = false;
        }

        private void FixedUpdate() {
            if (_enabled) Illuminate();
        }

        public override void AbilityStart() {
            _enabled = !_enabled;
            foreach (MeshRenderer meshRenderer in _meshRenderers) meshRenderer.enabled = _enabled;
            PlayerAnimation.Instance.Hold(_enabled);
            if (!_enabled) {
                foreach (ILightable lightable in _illuminatedObjects) lightable.IluminateStop();
                _illuminatedObjects.Clear();
                OnIlluminate.Invoke(Vector3.zero); // Maybe remove
            }

            Logger.Log(LogType.Player, $"Light {(_enabled ? "enabled" : "disabled")}");
        }

        public override void AbilityEnd() { }

        private void Illuminate() {
            OnIlluminate.Invoke(Physics.Raycast(_origin.position, _origin.forward, out RaycastHit hitLine, _lightDistance, _lightableLayer) ? hitLine.point : Vector3.zero);

            _stopIlluminating.Clear();
            _stopIlluminating.UnionWith(_illuminatedObjects);

            ILightable lightable;
            for (int i = 0; i < Physics.OverlapSphereNonAlloc(_origin.position, _lightDistance, _lightHits, _lightableLayer); i++) {
                if (_lightHits[i] != null && _lightHits[i].TryGetComponent(out lightable)) {
                    Vector3 toTarget = _lightHits[i].transform.position - _origin.position;
                    if (Vector3.Angle(_origin.forward, toTarget.normalized) <= _coneAngleHalf) {
                        if (!Physics.Raycast(_origin.position, toTarget.normalized, toTarget.magnitude, _occlusionLayer)) {
                            if (_illuminatedObjects.Add(lightable)) lightable.Iluminate();
                            _stopIlluminating.Remove(lightable);
                        }
                    }
                }
            }

            foreach (ILightable lightableToStop in _stopIlluminating) {
                lightableToStop.IluminateStop();
                _illuminatedObjects.Remove(lightableToStop);
            }
        }

    }
}