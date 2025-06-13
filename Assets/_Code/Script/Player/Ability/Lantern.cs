using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Ivayami.Enemy;
using Ivayami.Save;
using Ivayami.UI;
using Default;

namespace Ivayami.Player.Ability
{


    public class Lantern : PlayerAbility
    {
        [SerializeField] private InventoryItem _item;
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

        //[SerializeField, Min(0f)] private float _durationMaxBase;
        [SerializeField, Min(0f)] private float _durationMaxCap;
        [SerializeField, Min(0f)] private float _durationIncreaseFromItem;
        [SerializeField, Range(0f, 1f), Tooltip("If the value is smaller than this percentage it will start flickering")] private float _flickeringStartTreshold;
        [SerializeField, Range(0f, 1f)] private float _flickeringChance;
        [SerializeField, Min(.02f)] private float _flickeringInterpolationDuration;
        [SerializeField, Min(0f)] private float[] _randomFlickeringIntensities;
        [SerializeField, Range(0f, 1f)] private float _reduceIntensityStartTreshold;
        [SerializeField, Min(0f)] private float _wideLightFinalIntensity;
        [SerializeField, Min(0f)] private float _focusedLightFinalIntensity;
        //private float _durationMax;
        private float _durationCurrent;

        [Header("Cache")]

        private HashSet<Lightable> _illuminatedObjects = new HashSet<Lightable>();
        private HashSet<Lightable> _stopIlluminating = new HashSet<Lightable>();
        private Collider[] _lightHits;

        private Transform _lightsOriginCurrent;
        private Coroutine _flickeringCoroutine;
        private float _coneAngleHalf;
        private float _lightDistance;
        private float _wideBaseIntensity;
        private float _focusedBaseIntensity;
        private bool _noFuel;
        public HashKeyBlocker ActivateBlocker { get; private set; } = new HashKeyBlocker();


        public const string ILLUMINATION_KEY = "Lantern";

        private void Awake()
        {
            _lightHits = new Collider[_lightMaxHitNumber];
            _behaviourCheckWait = new WaitForSeconds(_behaviourCheckInterval);
            _lightsOriginCurrent = _wideOrigin.transform;
            PlayerMovement.Instance.AddAdditionalVisuals(HandleUpdateVisuals);
            _visuals.gameObject.SetActive(false);
            //_durationMax = Mathf.Clamp(_durationMaxBase * (_durationIncreaseFromItem * PlayerInventory.Instance.CheckInventoryFor("ID").Amount), 0f, _durationMaxCap); // Change the ID for the proper ID
            _wideBaseIntensity = _wideOrigin.intensity;
            _focusedBaseIntensity = _focusedOrigin.intensity;

            Focus(false);
        }

        private void Start()
        {
            SavePoint.onSaveGameWithAnimation.AddListener(HandleSaveBlock);
            SavePoint.onSaveSequenceEnd.AddListener(HandleSaveAllow);
            PlayerUseItemUI.Instance.OnShowUI.AddListener(HandleUseItemUIBlock);
            PlayerUseItemUI.Instance.OnHideUI.AddListener(HandleUseItemUIAllow);
            PlayerUseItemUI.Instance.OnItemActivation.AddListener(HandleUseItemUIAllow);
        }

        private void Update()
        {
            if (!_enabled || !ActivateBlocker.IsAllowed) return;
            if (_focused)
            {
                _visuals.localRotation = Quaternion.Euler(PlayerCamera.Instance.MainCamera.transform.eulerAngles.x, 0f, 0f);
                _durationCurrent = Mathf.Clamp(_durationCurrent - _focusedDurationComsumptionMultiplier * Time.deltaTime, 0f, _durationMaxCap);
            }
            else _durationCurrent = Mathf.Clamp(_durationCurrent - Time.deltaTime, 0f, _durationMaxCap);
            UpdateLights();
        }

        private void OnDestroy()
        {
            SavePoint.onSaveGameWithAnimation.RemoveListener(HandleSaveBlock);
            SavePoint.onSaveSequenceEnd.RemoveListener(HandleSaveAllow);
            PlayerUseItemUI.Instance.OnShowUI.RemoveListener(HandleUseItemUIBlock);
            PlayerUseItemUI.Instance.OnHideUI.RemoveListener(HandleUseItemUIAllow);
            PlayerUseItemUI.Instance.OnItemActivation.RemoveListener(HandleUseItemUIAllow);
            Destroy(_focusedOrigin);
        }

        private IEnumerator CheckInterval()
        {
            while (true)
            {
                Illuminate();
                GravityRotate();

                yield return _behaviourCheckWait;
            }
        }

        private void Setup()
        {
            ActivateBlocker.OnAllow.AddListener(AllowActivate);
            ActivateBlocker.OnBlock.AddListener(PreventActivateRemember);

            PlayerActions.Instance.onLanternFocus.AddListener(Focus);
            PlayerStress.Instance.onFail.AddListener(() => { if (_enabled) AbilityStart(); });
            _focusedOrigin.transform.parent = PlayerCamera.Instance.MainCamera.transform;
            _focusedOrigin.transform.localPosition = _focusedSourceDistance * Vector3.forward;
            _focusedOrigin.transform.localRotation = Quaternion.identity;
            _focusedOrigin.enabled = false;
        }

        public override void AbilityStart()
        {
            if (!ActivateBlocker.IsAllowed || _durationCurrent <= 0) return;
            _enabled = !_enabled;
            Toggle(_enabled);
        }

        public override void AbilityEnd() { }

        private void Toggle(bool enabled)
        {
            if (_focusedOrigin.transform.localPosition.z == 0) Setup(); //

            _enabled = enabled;
            _visuals.gameObject.SetActive(_enabled);
            PlayerAnimation.Instance.Hold(_enabled);
            if (_enabled) StartCoroutine(CheckInterval());
            else
            {
                ClearAllLightData();
                StopAllCoroutines();
            }
        }

        private void Focus(bool isFocusing)
        {
            if (_noFuel) return;
            if (isFocusing && (!_enabled || !ActivateBlocker.IsAllowed)) return;
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

        public void Fill(float fillAmount)
        {
            _durationCurrent = Mathf.Clamp(_durationCurrent + fillAmount, 0f, _durationMaxCap);
            if(_durationCurrent > 0)
            {
                if (_noFuel)
                {
                    _noFuel = false;
                    _wideOrigin.enabled = !_focused;
                    _focusedOrigin.enabled = _focused;
                    ActivateBlocker.Toggle(nameof(Lantern), true);
                }
                _wideOrigin.intensity = _wideBaseIntensity;
                _focusedOrigin.intensity = _focusedBaseIntensity;
                if (_flickeringCoroutine != null)
                {
                    StopCoroutine(_flickeringCoroutine);
                    _flickeringCoroutine = null;
                }
            }
            else
            {
                _noFuel = true;
                DisableLantern();
            }
            Bag.Instance.UpdateItemDisplayText(_item, $"{(100 * _durationCurrent / _durationMaxCap).ToString("0.")}");
        }

        private void UpdateLights()
        {
            if (_durationCurrent > 0 && !_noFuel)
            {
                if (_durationCurrent <= _reduceIntensityStartTreshold * _durationMaxCap && _durationCurrent > _flickeringStartTreshold * _durationMaxCap)
                {
                    float count = 1f - (_durationCurrent / (_durationMaxCap * _reduceIntensityStartTreshold));
                    _wideOrigin.intensity = Mathf.Lerp(_wideBaseIntensity, _wideLightFinalIntensity, count);
                    _focusedOrigin.intensity = Mathf.Lerp(_focusedBaseIntensity, _focusedLightFinalIntensity, count);
                }

                if (_durationCurrent <= _flickeringStartTreshold * _durationMaxCap)
                {
                    if (Random.Range(0f, 1f) <= _flickeringChance && _flickeringCoroutine == null)
                    {
                        _flickeringCoroutine = StartCoroutine(FlickeringInterpolationCoroutine());
                    }
                }
            }
            else if (!_noFuel)
            {
                _noFuel = true;
                DisableLantern();
            }
            Bag.Instance.UpdateItemDisplayText(_item, $"{(100 * _durationCurrent / _durationMaxCap).ToString("0.")}");
        }

        private void DisableLantern()
        {
            ClearAllLightData();
            _wideOrigin.enabled = false;
            _focusedOrigin.enabled = false;
            ActivateBlocker.Toggle(nameof(Lantern), false);
        }

        private IEnumerator FlickeringInterpolationCoroutine()
        {
            float count = 0;
            float intensity = _randomFlickeringIntensities[Random.Range(0, _randomFlickeringIntensities.Length)];
            float wideBaseIntensity = _wideOrigin.intensity;
            float focusedBaseIntensity = _focusedOrigin.intensity;
            while (count < 1)
            {
                count += Time.deltaTime / _flickeringInterpolationDuration;
                _wideOrigin.intensity = Mathf.Lerp(wideBaseIntensity, intensity, count);
                _focusedOrigin.intensity = Mathf.Lerp(focusedBaseIntensity, intensity, count);
                if (count >= 1)
                {
                    _wideOrigin.intensity = intensity;
                    _focusedOrigin.intensity = intensity;
                }
                yield return null;
            }
            _flickeringCoroutine = null;
        }

        private void ClearAllLightData()
        {
            Focus(false);
            foreach (Lightable lightable in _illuminatedObjects) lightable.Illuminate(ILLUMINATION_KEY, false);
            _illuminatedObjects.Clear();
            LightFocuses.Instance.LightPointFocusRemove(ILLUMINATION_KEY);
            if (_flickeringCoroutine != null)
            {
                StopCoroutine(_flickeringCoroutine);
                _flickeringCoroutine = null;
            }
        }

        private void Illuminate()
        {
            if (_noFuel) return;
            if (Physics.Raycast(_lightsOriginCurrent.position, _lightsOriginCurrent.forward, out RaycastHit hitLine, _lightDistance, _lightableLayer))
                LightFocuses.Instance.LightPointFocusUpdate(ILLUMINATION_KEY, new LightFocuses.LightData(hitLine.point));
            else
                LightFocuses.Instance.LightPointFocusRemove(ILLUMINATION_KEY);

            _stopIlluminating.Clear();
            _stopIlluminating.UnionWith(_illuminatedObjects);

            Lightable lightable;
            for (int i = 0; i < Physics.OverlapSphereNonAlloc(_lightsOriginCurrent.position, _lightDistance, _lightHits, _lightableLayer); i++)
            {
                if (_lightHits[i] != null && _lightHits[i].TryGetComponent(out lightable))
                {
                    Vector3 toTarget = _lightHits[i].transform.position - _lightsOriginCurrent.position;
                    if (Vector3.Angle(_lightsOriginCurrent.forward, toTarget.normalized) <= _coneAngleHalf)
                    {
                        if (!Physics.Raycast(_lightsOriginCurrent.position, toTarget.normalized, toTarget.magnitude, _occlusionLayer))
                        {
                            if (_illuminatedObjects.Add(lightable)) lightable.Illuminate(ILLUMINATION_KEY, true);
                            _stopIlluminating.Remove(lightable);
                        }
                    }
                }
            }

            foreach (Lightable lightableToStop in _stopIlluminating)
            {
                lightableToStop.Illuminate(ILLUMINATION_KEY, false);
                _illuminatedObjects.Remove(lightableToStop);
            }
        }

        public void ForceTurnOff()
        {
            if (_enabled) AbilityStart();
        }

        private void AllowActivate()
        {
            if (_enabled) Toggle(true);
        }

        private void PreventActivateRemember()
        {
            if (_enabled) Toggle(false);
        }

        private void GravityRotate()
        {
            transform.rotation = Quaternion.AngleAxis(transform.parent.eulerAngles.y, Vector3.up);
        }

        private void HandleUpdateVisuals(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        #region Event Handlers
        private void HandleUseItemUIAllow()
        {
            ActivateBlocker.Toggle(PlayerUseItemUI.BLOCKER_KEY, true);
        }

        private void HandleUseItemUIBlock()
        {
            ActivateBlocker.Toggle(PlayerUseItemUI.BLOCKER_KEY, false);
        }

        private void HandleSaveAllow()
        {
            ActivateBlocker.Toggle(SavePoint.BLOCKER_KEY, true);
        }

        private void HandleSaveBlock()
        {
            ActivateBlocker.Toggle(PlayerUseItemUI.BLOCKER_KEY, false);
        }
        #endregion

#if UNITY_EDITOR
        [Header("Debug")]

        [SerializeField] private Color _coneColor;
        private Mesh _coneMesh;

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                //if(_coneMesh == default) _coneMesh = DebugUtilities.CreateConeMesh(transform, _coneAngleHalf * 2f, _lightDistance); // Doesnt work for rotation for some reason so eeeh
                _coneMesh = DebugUtilities.CreateConeMesh(transform, _coneAngleHalf * 2f, _lightDistance);
                Gizmos.color = _coneColor;
                Gizmos.DrawMesh(_coneMesh, _lightsOriginCurrent.transform.position, _focused ? Quaternion.Euler(PlayerCamera.Instance.MainCamera.transform.eulerAngles.x, 0, 0) : Quaternion.identity);

                Lightable lightable;
                for (int i = 0; i < Physics.OverlapSphereNonAlloc(_lightsOriginCurrent.position, _lightDistance, _lightHits, _lightableLayer); i++)
                {
                    if (_lightHits[i] != null && _lightHits[i].TryGetComponent(out lightable))
                    {
                        Vector3 toTarget = _lightHits[i].transform.position - _lightsOriginCurrent.position;
                        if (Vector3.Angle(_lightsOriginCurrent.forward, toTarget.normalized) <= _coneAngleHalf)
                        {
                            Gizmos.color = Physics.Raycast(_lightsOriginCurrent.position, toTarget.normalized, toTarget.magnitude, _occlusionLayer) ? Color.red : Color.green;
                            Gizmos.DrawLine(_lightsOriginCurrent.position, lightable.transform.position);
                        }
                    }
                }
            }
        }

        private void OnValidate()
        {
            if (_reduceIntensityStartTreshold < _flickeringStartTreshold) _reduceIntensityStartTreshold = _flickeringStartTreshold;
        }
#endif

    }
}