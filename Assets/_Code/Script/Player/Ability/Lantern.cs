using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
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
        [SerializeField, Min(1f)] private float _focusedDurationComsumptionMultiplier;
        [SerializeField, Min(1f)] private float _focusedSourceDistance;

        [Header("Header")]

        [SerializeField, Min(0f)] private float _durationMaxBase;
        [SerializeField, Min(0f)] private float _durationIncreaseFromItem;
        private float _durationMax;
        private float _durationCurrent;

        [Header("Cache")]

        private HashSet<Lightable> _illuminatedObjects = new HashSet<Lightable>();
        private HashSet<Lightable> _stopIlluminating = new HashSet<Lightable>();
        private Collider[] _lightHits;

        private Transform _lightsOriginCurrent;
        private float _coneAngleHalf;
        private float _lightDistance;


        public const string ILLUMINATION_KEY = "Lantern";

        private void Awake() {
            _lightHits = new Collider[_lightMaxHitNumber];
            _behaviourCheckWait = new WaitForSeconds(_behaviourCheckInterval);
            _lightsOriginCurrent = _wideOrigin.transform;
            PlayerMovement.Instance.AddAdditionalVisuals(GetComponentsInChildren<MeshRenderer>());
            _visuals.gameObject.SetActive(false);
            _durationMax = _durationMaxBase * (_durationIncreaseFromItem * PlayerInventory.Instance.CheckInventoryFor("ID").Amount); // Change the ID for the proper ID

            Focus(false);
        }

        private void Update() {
            if (!_enabled) return;
            if (_focused) {
                _visuals.localRotation = Quaternion.Euler(PlayerCamera.Instance.MainCamera.transform.eulerAngles.x, 0f, 0f);
                _durationCurrent -= Time.deltaTime;
            }
            _durationCurrent -= _focusedDurationComsumptionMultiplier * Time.deltaTime;
        }

        private void OnDestroy() {
            Destroy(_focusedOrigin);
        }

        private IEnumerator CheckInterval() {
            while (true) {
                Illuminate();
                GravityRotate();

                yield return _behaviourCheckWait;
            }
        }

        private void Setup() {
            PlayerActions.Instance.onLanternFocus.AddListener(Focus);
            PlayerStress.Instance.onFail.AddListener(() => { if (_enabled) AbilityStart(); });
            _focusedOrigin.transform.parent = PlayerCamera.Instance.MainCamera.transform;
            _focusedOrigin.transform.localPosition = _focusedSourceDistance * Vector3.forward;
            _focusedOrigin.transform.localRotation = Quaternion.identity;
            _focusedOrigin.enabled = false;
        }

        public override void AbilityStart() {
            if (_focusedOrigin.transform.localPosition.z == 0) Setup(); // temp
            _enabled = !_enabled;
            _visuals.gameObject.SetActive(_enabled);
            PlayerAnimation.Instance.Hold(_enabled);
            if (_enabled) StartCoroutine(CheckInterval());
            else {
                Focus(false);
                StopAllCoroutines();
                foreach (Lightable lightable in _illuminatedObjects) lightable.Illuminate(ILLUMINATION_KEY, false);
                _illuminatedObjects.Clear();
                LightFocuses.Instance.LightPointFocusRemove(ILLUMINATION_KEY);
            }
        }

        public override void AbilityEnd() { }

        private void Illuminate() {
            if (Physics.Raycast(_lightsOriginCurrent.position, _lightsOriginCurrent.forward, out RaycastHit hitLine, _lightDistance, _lightableLayer))
                LightFocuses.Instance.LightPointFocusUpdate(ILLUMINATION_KEY, new LightFocuses.LightData(hitLine.point));
            else
                LightFocuses.Instance.LightPointFocusRemove(ILLUMINATION_KEY);

            _stopIlluminating.Clear();
            _stopIlluminating.UnionWith(_illuminatedObjects);

            Lightable lightable;
            for (int i = 0; i < Physics.OverlapSphereNonAlloc(_lightsOriginCurrent.position, _lightDistance, _lightHits, _lightableLayer); i++) {
                if (_lightHits[i] != null && _lightHits[i].TryGetComponent(out lightable)) {
                    Vector3 toTarget = _lightHits[i].transform.position - _lightsOriginCurrent.position;
                    if (Vector3.Angle(_lightsOriginCurrent.forward, toTarget.normalized) <= _coneAngleHalf) {
                        if (!Physics.Raycast(_lightsOriginCurrent.position, toTarget.normalized, toTarget.magnitude, _occlusionLayer)) {
                            if (_illuminatedObjects.Add(lightable)) lightable.Illuminate(ILLUMINATION_KEY, true);
                            _stopIlluminating.Remove(lightable);
                        }
                    }
                }
            }

            foreach (Lightable lightableToStop in _stopIlluminating) {
                lightableToStop.Illuminate(ILLUMINATION_KEY, false);
                _illuminatedObjects.Remove(lightableToStop);
            }
        }

        private void GravityRotate() {
            transform.rotation = Quaternion.AngleAxis(transform.parent.eulerAngles.y, Vector3.up);
        }

        private void Focus(bool isFocusing) {
            if (isFocusing && !_enabled) return;
            _focused = isFocusing;
            _wideOrigin.enabled = !_focused;
            _focusedOrigin.enabled = _focused;
            Light light = (_focused ? _focusedOrigin : _wideOrigin);
            _coneAngleHalf = light.spotAngle / 2f;
            _lightDistance = light.range;
            _lightsOriginCurrent = light.transform;
            PlayerCamera.Instance.SetOrbits(_focused ? _focusedCamOrbits : null);
            CameraAimReposition.Instance.SetMaxDistance(_focused ? _focusedCamArmDistance : 0f);
            PlayerMovement.Instance.AllowRun(!isFocusing);
            PlayerMovement.Instance.useCameraRotaion = _focused;
            if (!_focused) _visuals.localRotation = Quaternion.identity;
        }

        public void Fill(float fillAmount) {
            _durationCurrent += fillAmount;
            if (_durationCurrent > _durationMax) _durationMax = _durationCurrent;
        }

#if UNITY_EDITOR
        [Header("Debug")]

        [SerializeField] private Color _coneColor;
        private Mesh _coneMesh;

        private void OnDrawGizmos() {
            if (Application.isPlaying) {
                //if(_coneMesh == default) _coneMesh = DebugUtilities.CreateConeMesh(transform, _coneAngleHalf * 2f, _lightDistance); // Doesnt work for rotation for some reason so eeeh
                _coneMesh = DebugUtilities.CreateConeMesh(transform, _coneAngleHalf * 2f, _lightDistance);
                Gizmos.color = _coneColor;
                Gizmos.DrawMesh(_coneMesh, _lightsOriginCurrent.transform.position, _focused ? Quaternion.Euler(PlayerCamera.Instance.MainCamera.transform.eulerAngles.x, 0, 0) : Quaternion.identity);

                Lightable lightable;
                for (int i = 0; i < Physics.OverlapSphereNonAlloc(_lightsOriginCurrent.position, _lightDistance, _lightHits, _lightableLayer); i++) {
                    if (_lightHits[i] != null && _lightHits[i].TryGetComponent(out lightable)) {
                        Vector3 toTarget = _lightHits[i].transform.position - _lightsOriginCurrent.position;
                        if (Vector3.Angle(_lightsOriginCurrent.forward, toTarget.normalized) <= _coneAngleHalf) {
                            Gizmos.color = Physics.Raycast(_lightsOriginCurrent.position, toTarget.normalized, toTarget.magnitude, _occlusionLayer) ? Color.red : Color.green;
                            Gizmos.DrawLine(_lightsOriginCurrent.position, lightable.transform.position);
                        }
                    }
                }
            }
        }
#endif

    }
}
