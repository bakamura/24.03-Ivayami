using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ivayami.Player;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks))]
    public sealed class RotatingPedestalPuzzle : MonoBehaviour, IInteractable
    {
        [SerializeField] private RotatingObjectData[] _rotatingObjects;
        [SerializeField, Min(0f)] private float _rotationAmount = 90;
        [SerializeField, Min(0f)] private float _rotationDuration;
        [SerializeField] private InventoryItem _itemUsed;
        [SerializeField] private InputActionReference _cancelInteractionInput;
        [SerializeField] private InputActionReference _navigateUIInput;
        [SerializeField] private InputActionReference _confirmInput;
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onCancelInteraction;
        [SerializeField] private bool _playNoSolutionEventOnceBySolution;
        [SerializeField] private UnityEvent _onNoSolutionMeet;
        [SerializeField] private PuzzleSolutionEvent[] _solutions;
#if UNITY_EDITOR
        [SerializeField] private bool _drawGizmos;
#endif

        private sbyte _currentPuzzleLayer;
        private sbyte _currentPuzzleObjectSelectedIndex;
        private float _currentInputCooldown;
        private bool _triggerNoSolution;
        private const float _inputCooldown = .1f;
        private InteractableFeedbacks _interatctableFeedbacks;
        private RotatingPedestalPuzzleObject _currentSelected;
        private List<RotatingPedestalPuzzleObject[]> _puzzleObjects;
        private byte[] _totalPuzzleObjectsInLayer;
        private Dictionary<int, Coroutine> _rotationAnimations = new Dictionary<int, Coroutine>();
#if UNITY_EDITOR
        private List<Vector3[]> _debugGizmoPositions;
#endif

        [Serializable]
        private struct SolutionData
        {
            public byte[] Solution;
        }
        [Serializable]
        private struct PuzzleSolutionEvent
        {
            [Tooltip("Will check in the order of the list filled in RotatingObjects")] public SolutionData[] PuzzleLayer;
            public UnityEvent OnActivate;
        }
        [Serializable]
        private struct RotatingObjectData
        {
            public Transform Transform;
#if UNITY_EDITOR
            public Color DebugColor;
            [Min(0)] public int DebugTextSize;
            [Min(0f)] public float DebugTextHeight;
#endif
        }

        public InteractableFeedbacks InteratctableFeedbacks
        {
            get
            {
                if (!_interatctableFeedbacks) _interatctableFeedbacks = GetComponent<InteractableFeedbacks>();
                return _interatctableFeedbacks;
            }
        }
#if UNITY_EDITOR
        private void Awake()
        {
            FillDebugGizmoPositions();
        }

        private void FillDebugGizmoPositions()
        {
            _debugGizmoPositions = new List<Vector3[]>();
            for (int i = 0; i < _rotatingObjects.Length; i++)
            {
                RotatingPedestalPuzzleObject[] totalAmountPossible = _rotatingObjects[i].Transform.GetComponentsInChildren<RotatingPedestalPuzzleObject>(true);
                _debugGizmoPositions.Add(new Vector3[totalAmountPossible.Length]);
                for (int a = 0; a < totalAmountPossible.Length; a++)
                {
                    if (totalAmountPossible[a].transform) _debugGizmoPositions[i][a] = totalAmountPossible[a].transform.position;
                }
            }
        }
#endif

        private void Setup()
        {
            if (_puzzleObjects == null)
            {
                _totalPuzzleObjectsInLayer = new byte[_rotatingObjects.Length];
                _puzzleObjects = new List<RotatingPedestalPuzzleObject[]>();
                for (int i = 0; i < _rotatingObjects.Length; i++)
                {
                    _puzzleObjects.Add(_rotatingObjects[i].Transform.GetComponentsInChildren<RotatingPedestalPuzzleObject>());
                    RotatingPedestalPuzzleObject[] totalAmountPossible = _rotatingObjects[i].Transform.GetComponentsInChildren<RotatingPedestalPuzzleObject>(true);
                    _totalPuzzleObjectsInLayer[i] = (byte)totalAmountPossible.Length;
                    for (int a = 0; a < totalAmountPossible.Length; a++)
                    {
                        if (totalAmountPossible[a].gameObject.activeSelf) totalAmountPossible[a].Index = (sbyte)a;
                    }
                }
            }
        }

        public void ForceInteract() => Interact();

        public PlayerActions.InteractAnimation Interact()
        {
            Setup();
            _onInteract?.Invoke();
            _interatctableFeedbacks.UpdateFeedbacks(false, true);
            UpdateInputs(true);
            SetCurrentSelected(_puzzleObjects[_currentPuzzleLayer][0]);
            return PlayerActions.InteractAnimation.Default;
        }

        private void HandleCancelInteraction(InputAction.CallbackContext obj)
        {
            ExitInteraction();
        }

        private void HandleNavigationUI(InputAction.CallbackContext obj)
        {
            if (Time.time - _currentInputCooldown < _inputCooldown) return;
            _currentInputCooldown = Time.time;
            Vector2 input = obj.ReadValue<Vector2>();
            if (Mathf.Abs(input.y) == 1)
            {
                _currentPuzzleLayer += (sbyte)input.y;
                LoopValueByArraySize(ref _currentPuzzleLayer, _rotatingObjects.Length);
                _currentPuzzleObjectSelectedIndex = 0;
                SetCurrentSelected(_puzzleObjects[_currentPuzzleLayer][_currentPuzzleObjectSelectedIndex]);
            }
            else if (Mathf.Abs(input.x) == 1)
            {
                _currentPuzzleObjectSelectedIndex += (sbyte)input.x;
                LoopValueByArraySize(ref _currentPuzzleObjectSelectedIndex, _puzzleObjects[_currentPuzzleLayer].Length);
                SetCurrentSelected(_puzzleObjects[_currentPuzzleLayer][_currentPuzzleObjectSelectedIndex]);
            }
        }

        private void HandleConfirmInput(InputAction.CallbackContext obj)
        {
            _currentSelected.UpdateItem(_itemUsed);
            CheckForSolutionsCompleted();
        }

        private void LoopValueByArraySize(ref sbyte valueToConstrain, int arraySize)
        {
            if (valueToConstrain < 0) valueToConstrain = (sbyte)(arraySize - 1);
            else if (valueToConstrain >= arraySize) valueToConstrain = 0;
        }

        public void ExitInteraction()
        {
            UpdateInputs(false);
            SetCurrentSelected(null);
            _interatctableFeedbacks.UpdateFeedbacks(true, true);
            _onCancelInteraction?.Invoke();
        }

        private void UpdateInputs(bool isActive)
        {
            if (isActive)
            {
                _navigateUIInput.action.performed += HandleNavigationUI;
                _cancelInteractionInput.action.started += HandleCancelInteraction;
                _confirmInput.action.started += HandleConfirmInput;
                PlayerActions.Instance.ChangeInputMap("Menu");
                PlayerActions.Instance.ToggleInteract(nameof(RotatingPedestalPuzzle), false);
            }
            else
            {
                _navigateUIInput.action.performed -= HandleNavigationUI;
                _cancelInteractionInput.action.started -= HandleCancelInteraction;
                _confirmInput.action.started -= HandleConfirmInput;
                PlayerActions.Instance.ChangeInputMap("Player");
                PlayerActions.Instance.ToggleInteract(nameof(RotatingPedestalPuzzle), true);
            }
        }

        private void SetCurrentSelected(RotatingPedestalPuzzleObject selected)
        {
            if (_currentSelected) _currentSelected.InteratctableFeedbacks.UpdateFeedbacks(false, true);
            _currentSelected = selected;
            if (_currentSelected) _currentSelected.InteratctableFeedbacks.UpdateFeedbacks(true, true);
        }

        public void RotateObject(int rotatingObjectIndex)
        {
            Setup();
            if (!_rotationAnimations.ContainsKey(rotatingObjectIndex))
            {
                _rotationAnimations.Add(rotatingObjectIndex, StartCoroutine(RotateCoroutine(rotatingObjectIndex)));
            }
        }

        private IEnumerator RotateCoroutine(int rotatingObjectIndex)
        {
            float count = 0;
            WaitForFixedUpdate delay = new WaitForFixedUpdate();
            float initialAngle = _rotatingObjects[rotatingObjectIndex].Transform.localEulerAngles.y;
            while (count < 1)
            {
                count += Time.fixedDeltaTime / _rotationDuration;
                _rotatingObjects[rotatingObjectIndex].Transform.localEulerAngles = new Vector3(0, Mathf.LerpAngle(initialAngle, initialAngle + _rotationAmount, count), 0);
                yield return delay;
            }
            _rotationAnimations.Remove(rotatingObjectIndex);
            UpdatePuzzleObjectsIndex(rotatingObjectIndex);
            CheckForSolutionsCompleted();
        }

        private void UpdatePuzzleObjectsIndex(int rotatingObjectIndex)
        {
            sbyte factor = (sbyte)(_rotationAmount > 0 ? -1 : 1);
            for (int i = 0; i < _puzzleObjects[rotatingObjectIndex].Length; i++)
            {
                _puzzleObjects[rotatingObjectIndex][i].Index += factor;
                LoopValueByArraySize(ref _puzzleObjects[rotatingObjectIndex][i].Index, _totalPuzzleObjectsInLayer[rotatingObjectIndex]);
            }
        }

        private void CheckForSolutionsCompleted()
        {
            bool foundMatchingPuzzleObjectIndex = false;
            byte puzzleSolutionsCompleted;
            for (int currentSolutionIndex = 0; currentSolutionIndex < _solutions.Length; currentSolutionIndex++)
            {
                puzzleSolutionsCompleted = 0;
                for (int puzzleLayer = 0; puzzleLayer < _solutions[currentSolutionIndex].PuzzleLayer.Length; puzzleLayer++)
                {
                    for (int puzzleObjectIndexToMatch = 0; puzzleObjectIndexToMatch < _solutions[currentSolutionIndex].PuzzleLayer[puzzleLayer].Solution.Length; puzzleObjectIndexToMatch++)
                    {
                        for (int puzzleObjectIndex = 0; puzzleObjectIndex < _puzzleObjects[puzzleLayer].Length; puzzleObjectIndex++)
                        {
                            foundMatchingPuzzleObjectIndex = false;
                            if (_puzzleObjects[puzzleLayer][puzzleObjectIndex].IsCorrect((sbyte)_solutions[currentSolutionIndex].PuzzleLayer[puzzleLayer].Solution[puzzleObjectIndexToMatch]))
                            {
                                foundMatchingPuzzleObjectIndex = true;
                                break;
                            }
                        }
                        if (!foundMatchingPuzzleObjectIndex) break;
                    }
                    if (_solutions[currentSolutionIndex].PuzzleLayer[puzzleLayer].Solution.Length == 0) puzzleSolutionsCompleted++;
                    else if (!foundMatchingPuzzleObjectIndex) break;
                    else puzzleSolutionsCompleted++;
                }
                if (puzzleSolutionsCompleted == _solutions[currentSolutionIndex].PuzzleLayer.Length)
                {
                    _solutions[currentSolutionIndex].OnActivate?.Invoke();
                    _triggerNoSolution = false;
                    return;
                }
            }
            if ((!_triggerNoSolution && _playNoSolutionEventOnceBySolution) || !_playNoSolutionEventOnceBySolution)
            {
                _onNoSolutionMeet?.Invoke();
                _triggerNoSolution = true;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_rotatingObjects == null || !_drawGizmos) return;
            if (!Application.isPlaying) FillDebugGizmoPositions();
            //RotatingPuzzleObject[] puzzleObjects;
            GUIStyle style = new GUIStyle("LargeLabel");
            Color guiColor;
            for (int i = 0; i < _debugGizmoPositions.Count; i++)
            {
                for (int a = 0; a < _debugGizmoPositions[i].Length; a++)
                {
                    Gizmos.color = _rotatingObjects[i].DebugColor;
                    style.fontSize = _rotatingObjects[i].DebugTextSize;
                    guiColor = GUI.contentColor;
                    GUI.contentColor = _rotatingObjects[i].DebugColor;
                    UnityEditor.Handles.Label(_debugGizmoPositions[i][a] + Vector3.up * _rotatingObjects[i].DebugTextHeight, a.ToString(), style);
                    GUI.contentColor = guiColor;
                }
            }
            //for (int i = 0; i < _rotatingObjects.Length; i++)
            //{
            //    if (_rotatingObjects[i].Transform)
            //    {
            //        puzzleObjects = _rotatingObjects[i].Transform.GetComponentsInChildren<RotatingPuzzleObject>(true);
            //        for (int a = 0; a < puzzleObjects.Length; a++)
            //        {
            //            Gizmos.color = _rotatingObjects[i].DebugColor;
            //            style.fontSize = _rotatingObjects[i].DebugTextSize;
            //            guiColor = GUI.contentColor;
            //            GUI.contentColor = _rotatingObjects[i].DebugColor;
            //            UnityEditor.Handles.Label(puzzleObjects[a].transform.position + Vector3.up * _rotatingObjects[i].DebugTextHeight, a.ToString(), style);
            //            GUI.contentColor = guiColor;
            //        }
            //    }
            //}
        }

        private void OnValidate()
        {
            if (_rotatingObjects == null || _solutions == null) return;
            int size;
            for (int i = 0; i < _solutions.Length; i++)
            {
                if (_solutions[i].PuzzleLayer.Length != _rotatingObjects.Length) Array.Resize(ref _solutions[i].PuzzleLayer, _rotatingObjects.Length);
                for (int a = 0; a < _solutions[i].PuzzleLayer.Length; a++)
                {
                    if (_rotatingObjects[a].Transform)
                    {
                        size = _rotatingObjects[a].Transform.GetComponentsInChildren<RotatingPedestalPuzzleObject>().Length;
                        if (_solutions[i].PuzzleLayer[a].Solution == null) _solutions[i].PuzzleLayer[a].Solution = new byte[1];
                        if (_solutions[i].PuzzleLayer[a].Solution.Length > size)
                            Array.Resize(ref _solutions[i].PuzzleLayer[a].Solution, size);
                    }
                }
            }
        }
#endif
    }
}