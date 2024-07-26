using UnityEngine;
using Ivayami.Puzzle;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

namespace Ivayami.Player.Ability {
    public class Lantern : PlayerAbility {

        [SerializeField] private Transform _origin;
        [SerializeField] private float _lightRadius;
        [SerializeField] private float _lightFocusedRadius;
        [SerializeField] private float _lightDistance;
        [SerializeField] private LayerMask _lightableLayer;
        private bool _enabled = false;
        private bool _focused = false;
        [SerializeField] private int _lightMaxHitNumber;

        private List<ILightable> _iluminatedObjects = new List<ILightable>();
        public static UnityEvent<Vector3> OnIlluminate { get; private set; } = new UnityEvent<Vector3>();

        private RaycastHit[] _lightCastHits;
        private Queue<ILightable> _lightCastQueue = new Queue<ILightable>();
        private List<ILightable> _ligthCastStopIlluminating = new List<ILightable>();
        private ILightable _ligthCastIterator;

        [Header("Cache")]

        private MeshRenderer[] _meshRenderers;

        private void Awake() {
            _lightCastHits = new RaycastHit[_lightMaxHitNumber];
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach(MeshRenderer meshRenderer in _meshRenderers) meshRenderer.enabled = false;
        }

        private void FixedUpdate() {
            if (_enabled) Illuminate();
        }

        public override void AbilityStart() {
            _enabled = !_enabled;
            foreach (MeshRenderer meshRenderer in _meshRenderers) meshRenderer.enabled = _enabled;
            PlayerAnimation.Instance.Hold(_enabled);
            if(!_enabled) OnIlluminate.Invoke(Vector3.zero);

            Logger.Log(LogType.Player, $"Light {(_enabled ? "enabled" : "disabled")}");
        }

        public override void AbilityEnd() { }

        private void Illuminate() {
            OnIlluminate.Invoke(Physics.Raycast(_origin.position, _origin.forward, out RaycastHit hitLine, _lightDistance, _lightableLayer) ? hitLine.point : Vector3.zero);
            Array.Clear(_lightCastHits, 0, _lightCastHits.Length);
            Physics.SphereCastNonAlloc(transform.position + (transform.forward * _lightRadius), _focused ? _lightFocusedRadius : _lightRadius, -transform.forward, _lightCastHits, _lightDistance - _lightRadius, _lightableLayer);
            foreach (RaycastHit hit in _lightCastHits) {
                ILightable lightableComponent;
                if (hit.transform != null) {
                lightableComponent = hit.transform.GetComponent<ILightable>();
                if (lightableComponent != null)_lightCastQueue.Enqueue(lightableComponent);
                }
            }
            _ligthCastStopIlluminating = _iluminatedObjects;
            while (_lightCastQueue.Count > 0) {
                _ligthCastIterator = _lightCastQueue.Dequeue();
                if (_iluminatedObjects.Contains(_ligthCastIterator)) _ligthCastStopIlluminating.Remove(_ligthCastIterator);
                else {
                    _ligthCastIterator.Iluminate();
                    _iluminatedObjects.Add(_ligthCastIterator);
                }
            }
            foreach (ILightable lightable in _ligthCastStopIlluminating) lightable.IluminateStop();
            _ligthCastStopIlluminating.Clear();
        }

    }
}