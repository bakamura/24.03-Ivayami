using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Ivayami.Player.Ability;
using Ivayami.Puzzle;
using System.Linq;

namespace Ivayami.Player {
    public class PlayerActions : MonoSingleton<PlayerActions> {

        [Header("Inputs")]

        [SerializeField] private InputActionReference _interactInput;
        [SerializeField] private InputActionReference _abilityInput;
        [SerializeField] private InputActionReference _changeAbilityInput;

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
        private IInteractable _interactableClosest;
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
        private float _interactableDistanceCache;

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

                        Logger.Log(LogType.Player, $"Interact Long with: {_interactableClosestCache.gameObject.name}");
                    }
                    else {
                        onInteract?.Invoke();

                        Logger.Log(LogType.Player, $"Interact with: {_interactableClosestCache.gameObject.name}");
                    }
                }
                else Logger.Log(LogType.Player, $"Interact: No Target");
            }
            else if (input.phase == InputActionPhase.Canceled && Interacting) {
                (InteractableTarget as IInteractableLong).InteractStop();
                onInteractLong?.Invoke(false);

                Logger.Log(LogType.Player, $"Stop Interact Long with: {_interactableClosestCache.gameObject.name}");
            }
        }

        private void InteractObjectDetect() {
            _interactableClosestDistanceCache = _interactRange;
            _interactableClosestCache = null;
            _raycastHitsCache = Physics.SphereCastAll(_cam.ScreenPointToRay(_screenCenter), _interactSphereCastRadius, Mathf.Infinity);
            foreach (RaycastHit hit in _raycastHitsCache) {
                _interactableClosest = hit.collider.GetComponent<IInteractable>();
                if (_interactableClosest != null) {
                    _interactableDistanceCache = InteractableTarget != null ? Vector3.Distance(transform.position, InteractableTarget.gameObject.transform.position) : 0;
                    if (_interactableClosestDistanceCache > _interactableDistanceCache) {
                        _interactableClosestDistanceCache = _interactableDistanceCache;
                        _interactableClosestCache = _interactableClosest;
                    }
                }
            }
            if (InteractableTarget != _interactableClosestCache) {
                InteractableTarget?.InteratctableHighlight.UpdateHighlight(false);
                InteractableTarget = _interactableClosestCache;
                InteractableTarget?.InteratctableHighlight.UpdateHighlight(true);
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
            _abilities.Add(ability);

            Logger.Log(LogType.Player, $"Ability Add: {ability.name}");
        }

        public void RemoveAbility(PlayerAbility ability) {
            if (_abilityCurrent >= _abilities.FindIndex((abilityInList) => abilityInList == ability)) _abilityCurrent--;
            _abilities.Remove(ability);
            onAbilityChange?.Invoke(_abilityCurrent); // Update UI etc

            Logger.Log(LogType.Player, $"Ability Remove: {ability.name}");
        }

        public void ChangeInputMap(string mapId) {
            foreach (InputActionMap actionMap in _interactInput.asset.actionMaps) actionMap.Disable(); // Change to memory current
            _interactInput.asset.actionMaps.FirstOrDefault(actionMap => actionMap.name == mapId).Enable();
        }

    }
}