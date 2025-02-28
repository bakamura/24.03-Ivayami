using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ivayami.Puzzle;
using Ivayami.Enemy;

namespace Ivayami.Player.Ability {
    public class Lantern : PlayerAbility {

        [SerializeField] private Transform _origin;
        [SerializeField] private float _lightRadius;
        [SerializeField] private float _lightFocusedRadius;
        [SerializeField] private float _lightDistance;
        [SerializeField] private LayerMask _lightableLayer;
        [SerializeField] private LayerMask _occlusionLayer;
        private bool _enabled = false;
        private bool _focused = false;
        [SerializeField] private int _lightMaxHitNumber;
        [SerializeField, Range(0.01f, 0.5f)] private float _behaviourCheckInterval;
        private WaitForSeconds _behaviourCheckWait;

        [Header("Cache")]

        private HashSet<Lightable> _illuminatedObjects = new HashSet<Lightable>();
        private HashSet<Lightable> _stopIlluminating = new HashSet<Lightable>();
        private Collider[] _lightHits;
        private float _coneAngleHalf;

        private MeshRenderer[] _meshRenderers;

        private const string ILLUMINATION_KEY = "Lantern";

        private void Awake() {
            _coneAngleHalf = GetComponentInChildren<Light>().spotAngle / 2f;
            _lightHits = new Collider[_lightMaxHitNumber];
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in _meshRenderers) meshRenderer.enabled = false;
            _behaviourCheckWait = new WaitForSeconds(_behaviourCheckInterval);
        }

        private IEnumerator CheckInterval() {
            while (true) {
                Illuminate();

                yield return _behaviourCheckWait;
            }
        }

        public override void AbilityStart() {
            _enabled = !_enabled;
            foreach (MeshRenderer meshRenderer in _meshRenderers) meshRenderer.enabled = _enabled;
            PlayerAnimation.Instance.Hold(_enabled);
            if (_enabled) StartCoroutine(CheckInterval());
            else {
                StopAllCoroutines();
                foreach (Lightable lightable in _illuminatedObjects) lightable.Iluminate(ILLUMINATION_KEY, false);
                _illuminatedObjects.Clear();
                LightFocuses.Instance.FocusRemove(ILLUMINATION_KEY);
            }
        }

        public override void AbilityEnd() { }

        private void Illuminate() {
            if (Physics.Raycast(_origin.position, _origin.forward, out RaycastHit hitLine, _lightDistance, _lightableLayer)) LightFocuses.Instance.FocusUpdate(ILLUMINATION_KEY, hitLine.point);
            else LightFocuses.Instance.FocusRemove(ILLUMINATION_KEY);

            _stopIlluminating.Clear();
            _stopIlluminating.UnionWith(_illuminatedObjects);

            Lightable lightable;
            for (int i = 0; i < Physics.OverlapSphereNonAlloc(_origin.position, _lightDistance, _lightHits, _lightableLayer); i++) {
                if (_lightHits[i] != null && _lightHits[i].TryGetComponent(out lightable)) {
                    Vector3 toTarget = _lightHits[i].transform.position - _origin.position;
                    if (Vector3.Angle(_origin.forward, toTarget.normalized) <= _coneAngleHalf) {
                        if (!Physics.Raycast(_origin.position, toTarget.normalized, toTarget.magnitude, _occlusionLayer)) {
                            if (_illuminatedObjects.Add(lightable)) lightable.Iluminate(ILLUMINATION_KEY, true);
                            _stopIlluminating.Remove(lightable);
                        }
                    }
                }
            }

            foreach (Lightable lightableToStop in _stopIlluminating) {
                lightableToStop.Iluminate(ILLUMINATION_KEY, false);
                _illuminatedObjects.Remove(lightableToStop);
            }
        }

    }
}
