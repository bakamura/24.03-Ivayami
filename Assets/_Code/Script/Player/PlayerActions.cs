using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Ivayami.Player.Ability;
using Ivayami.Puzzle;

namespace Ivayami.Player {
    public class PlayerActions : MonoSingleton<PlayerActions> {

        [Header("Inputs")]

        [SerializeField] private InputActionReference _interactInput;
        [SerializeField] private InputActionReference _abilityInput;
        [SerializeField] private InputActionReference _changeAbilityInput;
        [SerializeField] private InputActionReference[] _pauseInputs;
        private InputActionMap _actionMapCurrent;

        [Header("Events")]

        public UnityEvent<InteractAnimation> onInteract = new UnityEvent<InteractAnimation>();
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

        public enum InteractAnimation {
            Default,
            EnterLocker,
            PullRope,
            PullLever,
            PushButton
        }

        [Header("Hand Item")]

        private GameObject _handItemCurrent;
        [field: SerializeField] public Transform HoldPointLeft { get; private set; }

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
                    InteractAnimation animation = InteractableTarget.Interact();
                    if (InteractableTarget is IInteractableLong) {
                        PlayerMovement.Instance.ToggleMovement(false);
                        onInteractLong?.Invoke(true);

                        Logger.Log(LogType.Player, $"Interact Long with: {InteractableTarget.gameObject.name}");
                    }
                    else {
                        onInteract?.Invoke(animation);

                        Logger.Log(LogType.Player, $"Interact with: {InteractableTarget.gameObject.name}");
                    }
                }
                else Logger.Log(LogType.Player, $"Interact: No Target");
            }
            else if (input.phase == InputActionPhase.Canceled && Interacting) {
                if (InteractableTarget is IInteractableLong) PlayerMovement.Instance.ToggleMovement(true);
                (InteractableTarget as IInteractableLong).InteractStop();                
                onInteractLong?.Invoke(false);

                Logger.Log(LogType.Player, $"Stop Interact Long with: {InteractableTarget.gameObject.name}");
            }
        }

        private void InteractObjectDetect() {
            _interactableClosestDistanceCache = _interactRange;
            _interactableClosestCache = null;
            _raycastHitsCache = Physics.SphereCastAll(_cam.ScreenPointToRay(_screenCenter), _interactSphereCastRadius, Mathf.Infinity);
            Vector2 playerPositionFlat = new Vector2(transform.position.x, transform.position.z); // Make Cache
            Vector2 hitPositionFlat = Vector2.zero; // Make Cache
            foreach (RaycastHit hit in _raycastHitsCache) {
                _interactableIterator = hit.collider.GetComponent<IInteractable>();
                if (_interactableIterator != null) {
                    hitPositionFlat[0] = hit.point.x;
                    hitPositionFlat[1] = hit.point.z;
                    _interactableDistanceIterator = Vector3.Distance(playerPositionFlat, hitPositionFlat);
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
                    if (_abilities.Count < 1) return;
                    _abilityCurrent = 0;
                    break;
                case Vector2 v2 when v2.Equals(Vector2.right):
                    if (_abilities.Count < 2) return;
                    _abilityCurrent = 1;
                    break;
                case Vector2 v2 when v2.Equals(Vector2.down):
                    if (_abilities.Count < 3) return;
                    _abilityCurrent = 2;
                    break;
                case Vector2 v2 when v2.Equals(Vector2.left):
                    if (_abilities.Count < 4) return;
                    _abilityCurrent = 3;
                    break;
            }
            onAbilityChange?.Invoke(_abilityCurrent);

            Logger.Log(LogType.Player, $"Ability Changed to: {_abilities[_abilityCurrent].name}");
        }

        public void AddAbility(PlayerAbility ability) {
            PlayerAbility abilityInstance = Instantiate(ability);
            Quaternion localRotation = abilityInstance.transform.rotation;
            abilityInstance.transform.parent = HoldPointLeft;
            abilityInstance.transform.localPosition = Vector3.zero;
            abilityInstance.transform.localRotation = localRotation;
            _abilities.Add(abilityInstance);

            Logger.Log(LogType.Player, $"Ability Add: {ability.name}");
        }

        public void RemoveAbility(PlayerAbility ability) {
            PlayerAbility abilityInList = _abilities.OrderBy(abilityIterator => abilityIterator.GetType() == ability.GetType()).First();
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
            _actionMapCurrent?.Disable();
            if (mapId != null) _actionMapCurrent = _interactInput.asset.actionMaps.FirstOrDefault(actionMap => actionMap.name == mapId);
            _actionMapCurrent?.Enable();
            Cursor.lockState = InputCallbacks.Instance.IsGamepad || mapId == "Player" ? CursorLockMode.Locked : CursorLockMode.None;
        }

    }
}