using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Ivayami.Player.Ability;
using Ivayami.Puzzle;
using System.Linq;
using System;

namespace Ivayami.Player {
    public class PlayerActions : MonoSingleton<PlayerActions> {

        [Header("Inputs")]

        [SerializeField] private InputActionReference _interactInput;
        [SerializeField] private InputActionReference _abilityInput;
        [SerializeField] private InputActionReference _changeAbilityInput;
        [SerializeField] private InputActionReference[] _pauseInputs;

        [Header("Events")]

        public UnityEvent onInteract = new UnityEvent();
        public UnityEvent<bool> onInteractLong = new UnityEvent<bool>();
        public UnityEvent<IInteractable> onInteractTargetChange = new UnityEvent<IInteractable>();
        public UnityEvent<string> onAbility = new UnityEvent<string>();
        public UnityEvent<sbyte> onAbilityChange = new UnityEvent<sbyte>();

        [Header("Interact")]

        [SerializeField] private float _interactRange;
        [SerializeField] private float _interactSphereCastRadius;
        [SerializeField] private LayerMask _interactLayer;
        private IInteractable _interactableIterator;
        public bool Interacting { get; private set; } = false;
        public IInteractable InteractableTarget { get; private set; }

        [Header("Hand Item")]

        private GameObject _handItemCurrent;

        [Header("Abilities")]

        private List<PlayerAbility> _abilities = new List<PlayerAbility>();
        private sbyte _abilityCurrent;

        [Header("Cache")]

        private Camera _cam;
        private Vector2 _screenCenter = new Vector2(Screen.width, Screen.height) / 2;
        private RaycastHit[] _raycastHitsCache;
        private IInteractable _interactableClosestCache;
        private float _interactableClosestDistanceCache;
        private float _interactableDistanceIterator;

        protected override void Awake() {
            base.Awake();

            _interactInput.action.started += Interact;
            _interactInput.action.canceled += Interact;
            _abilityInput.action.started += Ability;
            _changeAbilityInput.action.started += ChangeAbility;

            onInteractLong.AddListener((interacting) => Interacting = interacting);

            _abilityCurrent = (sbyte)(_abilities.Count > 0 ? 0 : -1); //

            _cam = Camera.main;

            Logger.Log(LogType.Player, $"{typeof(PlayerActions).Name} Initialized");
        }

        private void Update() {
            if (!Interacting) InteractObjectDetect();
        }

        private void Interact(InputAction.CallbackContext input) {
            if (input.phase == InputActionPhase.Started) {
                if (InteractableTarget != null && InteractableTarget != Friend.Instance?.InteractableLongCurrent) {
                    InteractableTarget.Interact();
                    if (InteractableTarget is IInteractableLong) {
                        onInteractLong?.Invoke(true);

                        Logger.Log(LogType.Player, $"Interact Long with: {InteractableTarget.gameObject.name}");
                    }
                    else {
                        onInteract?.Invoke();

                        Logger.Log(LogType.Player, $"Interact with: {InteractableTarget.gameObject.name}");
                    }
                }
                else Logger.Log(LogType.Player, $"Interact: No Target");
            }
            else if (input.phase == InputActionPhase.Canceled && Interacting) {
                (InteractableTarget as IInteractableLong).InteractStop();
                onInteractLong?.Invoke(false);

                Logger.Log(LogType.Player, $"Stop Interact Long with: {InteractableTarget.gameObject.name}");
            }
        }

        private void InteractObjectDetect() {
            _interactableClosestDistanceCache = _interactRange;
            _interactableClosestCache = null;
            _raycastHitsCache = Physics.SphereCastAll(_cam.ScreenPointToRay(_screenCenter), _interactSphereCastRadius, Mathf.Infinity);
            foreach (RaycastHit hit in _raycastHitsCache) {
                _interactableIterator = hit.collider.GetComponent<IInteractable>();
                if (_interactableIterator != null) {
                    _interactableDistanceIterator = Vector3.Distance(transform.position, _interactableIterator.gameObject.transform.position);
                    if (_interactableClosestDistanceCache > _interactableDistanceIterator) {
                        _interactableClosestDistanceCache = _interactableDistanceIterator;
                        _interactableClosestCache = _interactableIterator;
                    }
                }
            }
            if (InteractableTarget != _interactableClosestCache) {
                InteractableTarget?.InteratctableHighlight.UpdateFeedbacks(false);
                InteractableTarget = _interactableClosestCache;
                InteractableTarget?.InteratctableHighlight.UpdateFeedbacks(true);
                onInteractTargetChange?.Invoke(InteractableTarget);
                Logger.Log(LogType.Player, $"Changed Current Interact Target to: {(InteractableTarget != null ? InteractableTarget.gameObject.name : "Null")}");
            }
        }

        private void Ability(InputAction.CallbackContext input) {
            if (_abilityCurrent >= 0) {
                if (input.phase == InputActionPhase.Started) {
                    _abilities[_abilityCurrent].AbilityStart();
                    onAbility?.Invoke(_abilities[_abilityCurrent].name);

                    Logger.Log(LogType.Player, $"Ability Start: {_abilities[_abilityCurrent].name}");
                }
                else if (input.phase == InputActionPhase.Canceled) {
                    _abilities[_abilityCurrent].AbilityEnd();
                    onAbility?.Invoke($"{_abilities[_abilityCurrent].name}End");

                    Logger.Log(LogType.Player, $"Ability End: {_abilities[_abilityCurrent].name}");
                }
            }
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
            onAbilityChange?.Invoke(_abilityCurrent);

            Logger.Log(LogType.Player, $"Ability Changed to: {_abilities[_abilityCurrent].name}");
        }

        public void AddAbility(PlayerAbility ability) {
            PlayerAbility abilityInstance = Instantiate(ability, PlayerMovement.Instance.transform);
            _abilities.Add(abilityInstance);

            Logger.Log(LogType.Player, $"Ability Add: {ability.name}");
        }

        public void RemoveAbility(PlayerAbility ability) {
            PlayerAbility abilityInList = _abilities.OrderBy(abilityIterator =>  abilityIterator.GetType() == ability.GetType()).First();
            if (_abilityCurrent >= _abilities.FindIndex((abilityIterator) => abilityIterator == abilityInList)) _abilityCurrent--;
            _abilities.Remove(abilityInList);
            onAbilityChange?.Invoke(_abilityCurrent); // Update UI etc

            Logger.Log(LogType.Player, $"Ability Remove: {ability.name}");
        }

        public bool CheckAbility(PlayerAbility abilityChecking) {
            Debug.Log(abilityChecking.GetType()); // DEBUG REMOVE
            foreach (PlayerAbility ability in _abilities) if (ability.GetType() == abilityChecking.GetType()) return true;
            return false;
        }

        public void ChangeInputMap(string mapId) {
            foreach (InputActionMap actionMap in _interactInput.asset.actionMaps) actionMap.Disable(); // Change to memory current
            if (mapId != null) _interactInput.asset.actionMaps.FirstOrDefault(actionMap => actionMap.name == mapId).Enable();
            Cursor.lockState = mapId != "Player" ? CursorLockMode.None : CursorLockMode.Locked;
        }

        public void AllowPausing(bool doAllow) {
            foreach (InputActionReference actionRef in _pauseInputs) {
                if (doAllow) actionRef.action.Enable();
                else actionRef.action.Disable();
            }
        }

    }
}