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
using Ivayami.Dialogue;

namespace Ivayami.Player {
    public class PlayerActions : MonoSingleton<PlayerActions> {

        [Header("Inputs")]

        [SerializeField] private InputActionReference _interactInput;
        [SerializeField] private InputActionReference _abilityInput;
        [SerializeField] private InputActionReference _changeAbilityInput;
        [SerializeField] private InputActionReference _useHealthItemInput;
        [SerializeField] private InputActionReference[] _pauseInputs;
        private InputActionMap _actionMapCurrent;

        public InputActionMap CurrentActionMap => _actionMapCurrent;

        [Header("Events")]

        public UnityEvent<InteractAnimation> onInteract = new UnityEvent<InteractAnimation>();
        public UnityEvent<bool> onInteractLong = new UnityEvent<bool>();
        public UnityEvent<IInteractable> onInteractTargetChange = new UnityEvent<IInteractable>();
        public UnityEvent<string> onAbility = new UnityEvent<string>();
        public UnityEvent<sbyte> onAbilityChange = new UnityEvent<sbyte>();
        public UnityEvent<string> onActionMapChange = new UnityEvent<string>();

        [Header("Interact")]

        [SerializeField] private InteractableDetector _interactableDetector;
        [SerializeField] private LayerMask _interactLayer;
        [SerializeField] private LayerMask _blockLayers;
        [SerializeField, Min(0f)] private float _interactableCheckDelay;
        private const float _interactCoodlown = .5f;
        //private float _currentInteractCooldown;
        private HashSet<string> _interactBlock = new HashSet<string>();
        private Dictionary<string, Coroutine> _interactReleaseDelay = new Dictionary<string, Coroutine>();

        public bool Interacting { get; private set; } = false;
        public IInteractable InteractableTarget { get; private set; } // Should be private now?

        public enum InteractAnimation {
            Default,
            EnterLocker,
            EnterTrash,
            Seat,
            PullRope,
            PullLever,
            PushButton,
            HeavyPickup,
            HeavyPlace
        }

        //[Header("Hand Item")]

        [field: SerializeField] public Transform HoldPointLeft { get; private set; }
        [field: SerializeField] public Transform HoldPointHeavy { get; private set; }

        [Header("Abilities")]

        private List<PlayerAbility> _abilities = new List<PlayerAbility>();
        private sbyte _abilityCurrent; //

        [Header("Heavy Objects")]

        private GameObject _heavyObjectCurrent;
        public bool IsHoldingHeavyObject { get { return _heavyObjectCurrent != null; } }

        [Header("Cache")]

        private CinemachineBrain _brain;
        private RaycastHit _interactableHitCache;
        private IInteractable _interactableClosestCache;
        private WaitForSeconds _interactableCheckWait;
        private InteractAnimation _interactAnimationCache;

        private const string INTERACT_LONG_BLOCK_KEY = "InteractLong";

        protected override void Awake() {
            base.Awake();

            _interactInput.action.started += Interact;
            _interactInput.action.canceled += Interact;
            _abilityInput.action.started += Ability;
            //_changeAbilityInput.action.started += ChangeAbility;
            _useHealthItemInput.action.started += UseHealthItem;
            foreach (InputActionMap actionMap in _interactInput.asset.actionMaps) actionMap.Disable();

            onInteractLong.AddListener((interacting) => Interacting = interacting);
            _interactableCheckWait = new WaitForSeconds(_interactableCheckDelay);

            _abilityCurrent = (sbyte)(_abilities.Count > 0 ? 0 : -1); //
        }

        private void Start() {
            DialogueController.Instance.OnDialogueStart += () => { if (DialogueController.Instance.LockInput) ToggleInteract("Dialogue", false); };
            DialogueController.Instance.OnDialogueEnd += () => { if (DialogueController.Instance.LockInput) ToggleInteract("Dialogue", true); };
            _brain = PlayerCamera.Instance.CinemachineBrain;
            StartCoroutine(InteractObjectDetect());
        }

        private void Interact(InputAction.CallbackContext input) {
            if (input.phase == InputActionPhase.Started && PlayerMovement.Instance.CanMove && _interactBlock.Count == 0) {
                if (InteractableTarget != null) {
                    _interactAnimationCache = InteractableTarget.Interact();
                    Vector3 directionToInteractable = InteractableTarget.InteratctableFeedbacks.IconPosition - transform.position;
                    PlayerMovement.Instance.SetTargetAngle(Mathf.Atan2(directionToInteractable[0], directionToInteractable[2]) * Mathf.Rad2Deg, false);
                    if (InteractableTarget is IInteractableLong) {
                        PlayerMovement.Instance.ToggleMovement(INTERACT_LONG_BLOCK_KEY, false);
                        onInteractLong?.Invoke(true);
                        onInteract?.Invoke(_interactAnimationCache);
                    }
                    else onInteract?.Invoke(_interactAnimationCache);
                }
            }
            else if (input.phase == InputActionPhase.Canceled && Interacting) {
                if (InteractableTarget is IInteractableLong) {
                    PlayerMovement.Instance.BlockMovementFor("Wait Animation End", PlayerAnimation.Instance.GetInteractAnimationDuration(_interactAnimationCache));
                    PlayerMovement.Instance.ToggleMovement(INTERACT_LONG_BLOCK_KEY, true);
                    Interacting = false;
                }
                (InteractableTarget as IInteractableLong).InteractStop();
                onInteractLong?.Invoke(false);
            }
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
                    }
                }

                yield return _interactableCheckWait;
            }
        }

        public void HeavyObjectHold(GameObject objToHold) {
            if (objToHold != null) {
                _heavyObjectCurrent = objToHold;
                _heavyObjectCurrent.transform.parent = HoldPointHeavy;
                _heavyObjectCurrent.transform.localPosition = Vector3.zero;
                _heavyObjectCurrent.transform.rotation = Quaternion.identity;
                if (_heavyObjectCurrent.TryGetComponent(out Collider collider)) collider.enabled = false;
                _interactableDetector.onlyHeavyObjects = true;
            }
        }

        public GameObject HeavyObjectRelease() {
            if (_heavyObjectCurrent.TryGetComponent(out Collider collider)) collider.enabled = true;
            GameObject releasedObject = _heavyObjectCurrent;
            _heavyObjectCurrent = null;
            _interactableDetector.onlyHeavyObjects = false;
            return releasedObject;
        }

        #region Abilities (To be removed)
        private void Ability(InputAction.CallbackContext input) {
            if (_abilityCurrent >= 0) {
                if (input.phase == InputActionPhase.Started) {
                    _abilities[_abilityCurrent].AbilityStart();
                    onAbility?.Invoke(_abilities[_abilityCurrent].GetType().Name);
                }
                else if (input.phase == InputActionPhase.Canceled) {
                    _abilities[_abilityCurrent].AbilityEnd();
                    onAbility?.Invoke($"{_abilities[_abilityCurrent].name}End");
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
        }

        public void AddAbility(PlayerAbility ability) {
            PlayerAbility abilityInstance = Instantiate(ability);
            Quaternion localRotation = abilityInstance.transform.rotation;
            abilityInstance.transform.parent = HoldPointLeft;
            abilityInstance.transform.localPosition = Vector3.zero;
            abilityInstance.transform.localRotation = localRotation;
            _abilities.Add(abilityInstance);

            if (_abilityCurrent < 0) _abilityCurrent = 0;
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

        private void UseHealthItem(InputAction.CallbackContext obj) {
            if (PlayerUseItemUI.Instance && !PlayerStress.Instance.FailState) PlayerUseItemUI.Instance.UpdateUI(!PlayerUseItemUI.Instance.IsActive);
        }

        public void ChangeInputMap(string mapId) {
            _actionMapCurrent?.Disable();
            _actionMapCurrent = mapId != null ? _interactInput.asset.actionMaps.FirstOrDefault(actionMap => actionMap.name == mapId) : null;
            _actionMapCurrent?.Enable();
            Cursor.lockState = InputCallbacks.Instance.IsGamepad || mapId == "Player" ? CursorLockMode.Locked : CursorLockMode.None;
            if (_actionMapCurrent != null) onActionMapChange?.Invoke(_actionMapCurrent.name);
        }

        public void ToggleInteract(string key, bool canInteract, float releaseDelay = _interactCoodlown) {
            if (canInteract) {
                if (!_interactBlock.Contains(key)) Debug.LogWarning($"'{key}' tried to unlock interact but key isn't blocking");
                else {
                    if (_interactReleaseDelay.ContainsKey(key)) Debug.LogWarning($"The object {key} asked multiple interact releases at the same time");
                    else _interactReleaseDelay.Add(key, StartCoroutine(ReleaseInteractDelay(key, releaseDelay)));
                }
            }
            else if (!_interactBlock.Add(key)) Debug.LogWarning($"'{key}' tried to lock interact but key is already blocking");
        }

        private IEnumerator ReleaseInteractDelay(string key, float delay) {
            yield return new WaitForSeconds(delay);
            _interactBlock.Remove(key);
            _interactReleaseDelay.Remove(key);
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