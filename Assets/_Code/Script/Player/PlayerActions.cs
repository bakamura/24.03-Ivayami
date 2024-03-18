using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Paranapiacaba.Player.Ability;
using UnityEngine.InputSystem;
using Paranapiacaba.Puzzle;

namespace Paranapiacaba.Player {
    public class PlayerActions : MonoSingleton<PlayerActions> {

        [Header("Inputs")]

        [SerializeField] private InputActionReference _interactInput;
        [SerializeField] private InputActionReference _abilityInput;
        [SerializeField] private InputActionReference _changeAbilityInput;

        [Header("Events")]

        public UnityEvent onInteractQuick = new UnityEvent();
        public UnityEvent<bool> onInteractLong = new UnityEvent<bool>();
        public UnityEvent<IInteractable> onInteractTargetChange = new UnityEvent<IInteractable>();
        public UnityEvent<string> onAbility = new UnityEvent<string>();
        public UnityEvent<byte> onAbilityChange = new UnityEvent<byte>();

        [Header("Interact")]

        [SerializeField] private Vector3 _interactOffset;
        [SerializeField] private float _interactRadius;
        [SerializeField] private LayerMask _interactLayer;
        private IInteractable _interactableClosest;
        private float _interactionQuickDuration;

        [Header("Abilities")]

        private List<PlayerAbility> _abilities;
        private byte _abilityCurrent;

        [Header("Cache")]

        private IInteractable _interactableClosestCache;
        private Collider _interactableClosestColliderCache;
        private float _interactableClosestDistanceCache;
        private IInteractable _interactableCache;
        private float _interactableDistanceCache;

        protected override void Awake() {
            base.Awake();

            ChangeInputMap(0);
            _interactInput.action.started += Interact;
            _abilityInput.action.started += Ability;
            _changeAbilityInput.action.started += ChangeAbility;

            _interactionQuickDuration = 0.1f; // Get animation duration

            Logger.Log(LogType.Player, $"{typeof(PlayerActions).Name} Initialized");
        }

        private void Update() {
            _interactableClosestCache = null;
            _interactableClosestDistanceCache = Mathf.Infinity;
            foreach (Collider col in Physics.OverlapSphere(transform.position + _interactOffset, _interactRadius)) {
                _interactableCache = col.GetComponent<IInteractable>();
                if (_interactableCache != null) {
                    _interactableDistanceCache = Vector3.Distance(transform.position, col.transform.position);
                    if (_interactableDistanceCache < _interactableClosestDistanceCache) {
                        _interactableClosestCache = _interactableCache;
                        _interactableClosestColliderCache = col;
                        _interactableClosestDistanceCache = _interactableDistanceCache;
                    }
                }
            }
            if (_interactableClosest != _interactableClosestCache) {
                _interactableClosest = _interactableClosestCache;
                onInteractTargetChange.Invoke(_interactableClosest);

                Logger.Log(LogType.Player, $"Changed Current Interact Target to: {_interactableClosestColliderCache.name}");
            }
        }

        private void Interact(InputAction.CallbackContext input) {
            // Interacts with current Interaction target
        }

        private void Ability(InputAction.CallbackContext input) {
            if (input.phase == InputActionPhase.Started) _abilities[_abilityCurrent].AbilityStart();
            else if (input.phase == InputActionPhase.Canceled) _abilities[_abilityCurrent].AbilityEnd();
        }

        private void ChangeAbility(InputAction.CallbackContext input) {
            switch (input.ReadValue<Vector2>()) {
                case Vector2 v2 when v2.Equals(Vector2.up):
                    _abilityCurrent = 0;
                    break;
                case Vector2 v2 when v2.Equals(Vector2.right):
                    _abilityCurrent = 1;
                    break;
                case Vector2 v2 when v2.Equals(Vector2.down):
                    _abilityCurrent = 2;
                    break;
                case Vector2 v2 when v2.Equals(Vector2.left):
                    _abilityCurrent = 3;
                    break;
            }
            onAbilityChange.Invoke(_abilityCurrent);
        }

        public void AddAbility(PlayerAbility ability) {
            _abilities.Add(ability);
        }

        public void RemoveAbility(PlayerAbility ability) {
            if (_abilityCurrent >= _abilities.FindIndex((abilityInList) => abilityInList == ability)) _abilityCurrent--;
            _abilities.Remove(ability);
            onAbilityChange.Invoke(_abilityCurrent); // Update UI etc
        }

        public void ChangeInputMap(byte mapId) {
            foreach (InputActionMap actionMap in _interactInput.asset.actionMaps) actionMap.Disable(); // Change to memory current
            _interactInput.asset.actionMaps[mapId].Enable();
        }

    }
}