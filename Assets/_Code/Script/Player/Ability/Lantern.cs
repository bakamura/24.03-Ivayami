using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Ivayami.Puzzle;
using Ivayami.Enemy;

namespace Ivayami.Player.Ability {
    public class Lantern : PlayerAbility {

        [SerializeField] private Light _wideOrigin;
        [SerializeField] private Light _focusedOrigin;
        [SerializeField] private Transform _visuals;
        [SerializeField] private LayerMask _lightableLayer;
        [SerializeField] private LayerMask _occlusionLayer;
        private bool _enabled = false;
        [SerializeField] private int _lightMaxHitNumber;
        [SerializeField, Range(0.01f, 0.5f)] private float _behaviourCheckInterval;
        private WaitForSeconds _behaviourCheckWait;

        [Header("Focus")]

        private bool _focused;
        [SerializeField] private float _focusedCamDistance;
        [SerializeField] private CinemachineFreeLook.Orbit[] _focusedCamOrbits;
        [SerializeField] private float _focusedCamArmDistance;

        [Header("Cache")]

        private HashSet<Lightable> _illuminatedObjects = new HashSet<Lightable>();
        private HashSet<Lightable> _stopIlluminating = new HashSet<Lightable>();
        private Collider[] _lightHits;

        private Transform _lightsParent;
        private float _coneAngleHalf;
        private float _lightDistance;


        private const string ILLUMINATION_KEY = "Lantern";

        private void Awake() {
            _lightHits = new Collider[_lightMaxHitNumber];
            _behaviourCheckWait = new WaitForSeconds(_behaviourCheckInterval);
            _lightsParent = _wideOrigin.transform.parent;
            _visuals.gameObject.SetActive(false);

            Focus(false);
        }

        private void Start() {
            PlayerActions.Instance.onLanternFocus.AddListener(Focus);
        }

        private void Update() {
            if (!_enabled || !_focused) return;
            _visuals.localRotation = Quaternion.Euler(PlayerCamera.Instance.MainCamera.transform.eulerAngles.x, 0f, 0f);
        }

        private IEnumerator CheckInterval() {
            while (true) {
                Illuminate();
                GravityRotate();

                yield return _behaviourCheckWait;
            }
        }

        public override void AbilityStart() {
            _enabled = !_enabled;
            _visuals.gameObject.SetActive(_enabled);
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
            if (Physics.Raycast(_lightsParent.position, _lightsParent.forward, out RaycastHit hitLine, _lightDistance, _lightableLayer)) LightFocuses.Instance.FocusUpdate(ILLUMINATION_KEY, hitLine.point);
            else LightFocuses.Instance.FocusRemove(ILLUMINATION_KEY);

            _stopIlluminating.Clear();
            _stopIlluminating.UnionWith(_illuminatedObjects);

            Lightable lightable;
            for (int i = 0; i < Physics.OverlapSphereNonAlloc(_lightsParent.position, _lightDistance, _lightHits, _lightableLayer); i++) {
                if (_lightHits[i] != null && _lightHits[i].TryGetComponent(out lightable)) {
                    Vector3 toTarget = _lightHits[i].transform.position - _lightsParent.position;
                    if (Vector3.Angle(_lightsParent.forward, toTarget.normalized) <= _coneAngleHalf) {
                        if (!Physics.Raycast(_lightsParent.position, toTarget.normalized, toTarget.magnitude, _occlusionLayer)) {
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

        private void GravityRotate() {
            transform.rotation = Quaternion.AngleAxis(transform.parent.eulerAngles.y, Vector3.up);
        }

        private void Focus(bool isFocusing) {
            if (!_enabled) return;
            _focused = isFocusing;
            _wideOrigin.enabled = !_focused;
            _focusedOrigin.enabled = _focused;
            Light light = (_focused ? _focusedOrigin : _wideOrigin);
            _coneAngleHalf = light.spotAngle / 2f;
            _lightDistance = light.range;
            PlayerCamera.Instance.SetOrbit(_focused ? _focusedCamOrbits : null);
            CameraAimReposition.Instance.SetMaxDistance(_focused ? _focusedCamArmDistance : 0f);
            if (!_focused) _visuals.localRotation = Quaternion.identity;
        }

    }
}
