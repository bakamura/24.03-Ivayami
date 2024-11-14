using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Cinemachine;
using Ivayami.Player.Ability;
using Ivayami.Puzzle;
using Ivayami.UI;

namespace Ivayami.Player {
    public class PlayerActions : MonoSingleton<PlayerActions> {

        [Header("Inputs")]

        [SerializeField] private InputActionReference _interactInput;
        [SerializeField] private InputActionReference _abilityInput;
        [SerializeField] private InputActionReference _changeAbilityInput;
        [SerializeField] private InputActionReference _useHealthItemInput;
        [SerializeField] private InputActionReference[] _pauseInputs;
        private InputActionMap _actionMapCurrent;

        [Header("Events")]

        public UnityEvent<InteractAnimation> onInteract = new UnityEvent<InteractAnimation>();
        public UnityEvent<bool> onInteractLong = new UnityEvent<bool>();
        public UnityEvent<IInteractable> onInteractTargetChange = new UnityEvent<IInteractable>();
        public UnityEvent<string> onAbility = new UnityEvent<string>();
        public UnityEvent<sbyte> onAbilityChange = new UnityEvent<sbyte>();

        [Header("Interact")]

        [SerializeField] private InteractableDetector _interactableDetector;
        [SerializeField] private LayerMask _interactLayer;
        [SerializeField] private LayerMask _blockLayers;
        [SerializeField] private float _interactableCheckDelay;

        public bool Interacting { get; private set; } = false;
        public IInteractable InteractableTarget { get; private set; } // Should be private now?

        public enum InteractAnimation {
            Default,
            EnterLocker,
            EnterTrash,
            PullRope,
            PullLever,
            PushButton
        }

        //[Header("Hand Item")]

        //private GameObject _handItemCurrent;
        [field: SerializeField] public Transform HoldPointLeft { get; private set; }

        [Header("Abilities")]

        private List<PlayerAbility> _abilities = new List<PlayerAbility>();
        private sbyte _abilityCurrent;

        [Header("Cache")]

        //private Camera _cam;
        private CinemachineBrain _brain;
        private RaycastHit _interactableHitCache;
        //private RaycastHit _blockerHitCache;
        private IInteractable _interactableClosestCache;
        private WaitForSeconds _interactableCheckWait;
        private InteractAnimation _interactAnimationCache;

        private const string INTERACT_LONG_BLOCK_KEY = "InteractLong";

        protected override void Awake() {
            base.Awake();

            _interactInput.action.started += Interact;
            _interactInput.action.canceled += Interact;
            _abilityInput.action.started += Ability;
            _changeAbilityInput.action.started += ChangeAbility;
            _useHealthItemInput.action.started += UseHealthItem;
            foreach (InputActionMap actionMap in _interactInput.asset.actionMaps) actionMap.Disable();

            onInteractLong.AddListener((interacting) => Interacting = interacting);
            _interactableCheckWait = new WaitForSeconds(_interactableCheckDelay);

            _abilityCurrent = (sbyte)(_abilities.Count > 0 ? 0 : -1); //

            Logger.Log(LogType.Player, $"{typeof(PlayerActions).Name} Initialized");
        }        

        private void Start() {
            //_cam = PlayerCamera.Instance.MainCamera;
            _brain = PlayerCamera.Instance.CinemachineBrain;
            StartCoroutine(InteractObjectDetect());
        }

        private void Interact(InputAction.CallbackContext input) {
            //if (PlayerMovement.Instance.CanMove) {
                if (input.phase == InputActionPhase.Started && PlayerMovement.Instance.CanMove) {
                    if (InteractableTarget != null /*&& InteractableTarget != Friend.Instance?.InteractableLongCurrent*/) {
                        _interactAnimationCache = InteractableTarget.Interact();
                        Vector3 directionToInteractable = InteractableTarget.gameObject.transform.position - transform.position;
                        PlayerMovement.Instance.SetTargetAngle(Mathf.Atan2(directionToInteractable[0], directionToInteractable[2]) * Mathf.Rad2Deg, false);
                        if (InteractableTarget is IInteractableLong) {
                            PlayerMovement.Instance.ToggleMovement(INTERACT_LONG_BLOCK_KEY, false);
                            onInteractLong?.Invoke(true);
                            onInteract?.Invoke(_interactAnimationCache);

                            Logger.Log(LogType.Player, $"Interact Long with: {InteractableTarget.gameObject.name}");
                        }
                        else {
                            onInteract?.Invoke(_interactAnimationCache);

                            Logger.Log(LogType.Player, $"Interact with: {InteractableTarget.gameObject.name}");
                        }
                    }
                    else Logger.Log(LogType.Player, $"Interact: No Target");
                }
                else if (input.phase == InputActionPhase.Canceled && Interacting) {
                if (InteractableTarget is IInteractableLong)
                {
                    PlayerMovement.Instance.BlockMovementFor("Wait Animation End", PlayerAnimation.Instance.GetInteractAnimationDuration(_interactAnimationCache));
                    PlayerMovement.Instance.ToggleMovement(INTERACT_LONG_BLOCK_KEY, true);
                    Interacting = false;
                }
                    (InteractableTarget as IInteractableLong).InteractStop();
                    onInteractLong?.Invoke(false);

                    Logger.Log(LogType.Player, $"Stop Interact Long with: {InteractableTarget.gameObject.name}");
                }
            //}
        }

        private IEnumerator InteractObjectDetect() {
            while (true) {
                if (!Interacting && _actionMapCurrent?.name == "Player" && !_brain.IsBlending) {
                    _interactableClosestCache = null;

                    IInteractable[] interactables = _interactableDetector.InteractablesDetected.OrderBy(interactable => Vector3.Distance(interactable.gameObject.transform.position, _interactableDetector.transform.position)).ToArray();
                    for (int i = 0; i < interactables.Length; i++) {
                        if (Physics.Raycast(_interactableDetector.transform.position, interactables[i].gameObject.transform.position - _interactableDetector.transform.position, out _interactableHitCache, 99f, _interactLayer)) {
                            if (!Physics.Raycast(_interactableDetector.transform.position, _interactableHitCache.point - _interactableDetector.transform.position, /*out _blockerHitCache,*/ Vector3.Distance(_interactableDetector.transform.position, _interactableHitCache.point), _blockLayers, QueryTriggerInteraction.Ignore)) {
                                _interactableClosestCache = interactables[i];
                                break;
                            }
                        }
                    }
                    if (InteractableTarget != _interactableClosestCache) {
                        InteractableTarget?.InteratctableFeedbacks.UpdateFeedbacks(false);
                        InteractableTarget = _interactableClosestCache;
                        InteractableTarget?.InteratctableFeedbacks.UpdateFeedbacks(true);
                        onInteractTargetChange?.Invoke(InteractableTarget);
                        Logger.Log(LogType.Player, $"Changed Current Interact Target to: {(InteractableTarget != null ? InteractableTarget.gameObject.name : "Null")}");
                    }
                }

                yield return _interactableCheckWait;
            }
        }

        #region Abilities (To be removed)
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
            foreach (PlayerAbility ability in _abilities) if (ability.GetType() == abilityChecking.GetType()) return true;
            return false;
        }
        #endregion

        private void UseHealthItem(InputAction.CallbackContext obj)
        {
            if (PlayerUseItemUI.Instance) PlayerUseItemUI.Instance.UpdateUI();
        }

        public void ChangeInputMap(string mapId) {
            _actionMapCurrent?.Disable();
            _actionMapCurrent = mapId != null ? _interactInput.asset.actionMaps.FirstOrDefault(actionMap => actionMap.name == mapId) : null;
            _actionMapCurrent?.Enable();
            Cursor.lockState = InputCallbacks.Instance.IsGamepad || mapId == "Player" ? CursorLockMode.Locked : CursorLockMode.None;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            IInteractable[] interactables = _interactableDetector.InteractablesDetected.OrderBy(interactable => Vector3.Distance(interactable.gameObject.transform.position, _interactableDetector.transform.position)).ToArray();
            for (int i = 0; i < interactables.Length; i++) {
                Physics.Raycast(_interactableDetector.transform.position, (interactables[i].gameObject.transform.position - _interactableDetector.transform.position), out RaycastHit hit, 99f, _interactLayer, QueryTriggerInteraction.Ignore);
                Physics.Raycast(_interactableDetector.transform.position, (hit.point - _interactableDetector.transform.position), out RaycastHit hit2, Vector3.Distance(_interactableDetector.transform.position, hit.point), _blockLayers, QueryTriggerInteraction.Ignore);

                Gizmos.color = hit.transform == null ? Color.yellow : (hit2.transform == null ? Color.green : Color.red);
                Gizmos.DrawRay(_interactableDetector.transform.position, (interactables[i].gameObject.transform.position - _interactableDetector.transform.position));
            }
        }
#endif
    }
}