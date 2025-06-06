using System;
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
using Ivayami.Save;
using UnityEngine.EventSystems;

namespace Ivayami.Player {
    public class PlayerActions : MonoSingleton<PlayerActions> {

        [Header("Inputs")]

        [SerializeField] private InputActionReference _interactInput;
        [SerializeField] private InputActionReference _abilityInput;
        [SerializeField] private InputActionReference _lanternFocusInput;
        [SerializeField] private InputActionReference _useHealthItemInput;
        [SerializeField] private InputActionReference[] _pauseInputs;
        private InputActionMap _actionMapCurrent;

        public InputActionMap CurrentActionMap => _actionMapCurrent;

        [Header("Events")]

        public UnityEvent<InteractAnimation> onInteract = new UnityEvent<InteractAnimation>();
        public UnityEvent<bool> onInteractLong = new UnityEvent<bool>();
        public UnityEvent<IInteractable> onInteractTargetChange = new UnityEvent<IInteractable>();
        public UnityEvent<string> onAbility = new UnityEvent<string>();
        public UnityEvent<bool> onLanternFocus = new UnityEvent<bool>();
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
            None,
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
        private const string HEAVY_HOLD_KEY = "HeavyHold";

        protected override void Awake() {
            base.Awake();

            _interactInput.action.started += Interact;
            _interactInput.action.canceled += Interact;
            _abilityInput.action.started += Ability;
            _lanternFocusInput.action.started += FocusToggle;
            _lanternFocusInput.action.canceled += FocusToggle;
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

        private void Interact(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Started && PlayerMovement.Instance.CanMove && _interactBlock.Count == 0) {
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
            else if (context.phase == InputActionPhase.Canceled && Interacting) {
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
                        if(InteractableTarget as MonoBehaviour) InteractableTarget.InteratctableFeedbacks.UpdateFeedbacks(false);
                        InteractableTarget = _interactableClosestCache;
                        InteractableTarget?.InteratctableFeedbacks.UpdateFeedbacks(true);
                        onInteractTargetChange?.Invoke(InteractableTarget);
                    }
                }

                yield return _interactableCheckWait;
            }
        }

        public void HeavyObjectHold(GameObject objToHold) {
            if (_heavyObjectCurrent == null) {
                if (objToHold != null) {
                    _heavyObjectCurrent = objToHold;
                    _heavyObjectCurrent.transform.parent = HoldPointHeavy;
                    _heavyObjectCurrent.transform.localPosition = Vector3.zero;
                    _heavyObjectCurrent.transform.localRotation = Quaternion.identity;
                    if (_heavyObjectCurrent.TryGetComponent(out Collider collider)) collider.enabled = false;
                    _interactableDetector.onlyHeavyObjects = true;
                }
                else Debug.LogWarning($"Tried to hold null object");
            }
            else Debug.LogWarning($"Tried to hold '{objToHold?.name}' but is alraedy holding '{_heavyObjectCurrent.name}'");
            (_abilities.FirstOrDefault(ability => ability is Lantern) as Lantern)?.ActivateBlocker.Toggle(HEAVY_HOLD_KEY, false);
        }

        public GameObject HeavyObjectRelease() {
            if (_heavyObjectCurrent) {
                if (_heavyObjectCurrent.TryGetComponent(out Collider collider)) collider.enabled = true;
                GameObject releasedObject = _heavyObjectCurrent;
                _heavyObjectCurrent = null;
                _interactableDetector.onlyHeavyObjects = false;
                (_abilities.FirstOrDefault(ability => ability is Lantern) as Lantern)?.ActivateBlocker.Toggle(HEAVY_HOLD_KEY, true);
                return releasedObject;
            }
            Debug.LogWarning($"Tried to release null object");
            return null;
        }

        #region Abilities
        private void Ability(InputAction.CallbackContext input) {
            if (_abilityCurrent >= 0 && !PlayerStress.Instance.StressIsMaxed) {
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

        public void AddAbility(PlayerAbility ability) {
            Quaternion localRotation = ability.transform.rotation;
            ability.transform.parent = HoldPointLeft;
            ability.transform.localPosition = Vector3.zero;
            ability.transform.localRotation = localRotation;
            _abilities.Add(ability);
            SaveSystem.Instance.Progress.AddAbility(ability.name);

            if (_abilityCurrent < 0) _abilityCurrent = 0;
        }

        public void RemoveAbility(PlayerAbility ability) {
            PlayerAbility abilityInList = _abilities.OrderBy(abilityIterator => abilityIterator.GetType() == ability.GetType()).First();
            if (_abilityCurrent >= _abilities.FindIndex((abilityIterator) => abilityIterator == abilityInList)) _abilityCurrent--;
            _abilities.Remove(abilityInList);
            SaveSystem.Instance.Progress.RemoveAbility(ability.name);

            Logger.Log(LogType.Player, $"Ability Remove: {ability.name}");
        }

        public bool CheckAbility(PlayerAbility abilityChecking) {
            foreach (PlayerAbility ability in _abilities) if (ability.GetType() == abilityChecking.GetType()) return true;
            return false;
        }

        public void ResetAbilities() {
            foreach (AbilityGiver giver in FindObjectsOfType<AbilityGiver>()) {
                foreach (PlayerAbility ability in _abilities)
                    if (ability.GetType().Name == giver.name) {
                        ability.transform.parent = giver.transform;
                        break;
                    }
            }
            _abilities.Clear();
            _abilityCurrent = -1;
        }
        #endregion

        private void UseHealthItem(InputAction.CallbackContext obj) {
            if (PlayerUseItemUI.Instance && !PlayerStress.Instance.FailState) PlayerUseItemUI.Instance.UpdateUI(!PlayerUseItemUI.Instance.IsActive);
        }

        public void ChangeInputMap(string mapId) {
            _actionMapCurrent?.Disable();
            _actionMapCurrent = mapId != null ? _interactInput.asset.actionMaps.FirstOrDefault(actionMap => actionMap.name == mapId) : null;
            _actionMapCurrent?.Enable();
            if(mapId != "Menu") EventSystem.current.SetSelectedGameObject(null);
            Cursor.lockState = InputCallbacks.Instance.IsGamepad || mapId == "Player" ? CursorLockMode.Locked : CursorLockMode.None;
            if (string.Equals(mapId, "Player")) EventSystem.current.SetSelectedGameObject(null);
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

        private void FocusToggle(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Started) onLanternFocus.Invoke(true);
            else if (context.phase == InputActionPhase.Canceled) onLanternFocus.Invoke(false);
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