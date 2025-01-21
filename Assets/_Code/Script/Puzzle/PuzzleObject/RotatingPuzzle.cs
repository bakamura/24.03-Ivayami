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
    public sealed class RotatingPuzzle : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform[] _rotatingObjects;
        [SerializeField, Min(0f)] private float _rotationAmount = 90;
        [SerializeField, Min(0f)] private float _rotationDuration;
        [SerializeField] private InventoryItem _itemUsed;
        [SerializeField] private InputActionReference _cancelInteractionInput;
        [SerializeField] private InputActionReference _navigateUIInput;
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onCancelInteraction;
        [SerializeField] private PuzzleSolutionData[] _solutions;

        private sbyte _currentPuzzleLayer;
        private sbyte _currentPuzzleObjectSelectedIndex;
        private List<RotatingPuzzleObject[]> _puzzleObjects;
        private InteractableFeedbacks _interatctableFeedbacks;
        private RotatingPuzzleObject _currentSelected;
        private Dictionary<int, Coroutine> _rotationAnimations = new Dictionary<int, Coroutine>();
        [Serializable]
        private struct PuzzleSolutionData
        {
            [Tooltip("Will check in the order of the list filled in RotatingObjects")] public List<byte[]> Solution;
            public UnityEvent OnActivate;
        }

        public InteractableFeedbacks InteratctableFeedbacks
        {
            get
            {
                if (!_interatctableFeedbacks) _interatctableFeedbacks = GetComponent<InteractableFeedbacks>();
                return _interatctableFeedbacks;
            }
        }

        private void Setup()
        {
            if (_puzzleObjects == null)
            {
                _puzzleObjects = new List<RotatingPuzzleObject[]>();
                for (int i = 0; i < _rotatingObjects.Length; i++)
                {
                    _puzzleObjects.Add(_rotatingObjects[i].GetComponentsInChildren<RotatingPuzzleObject>());
                    RotatingPuzzleObject[] totalAmountPossible = _rotatingObjects[i].GetComponentsInChildren<RotatingPuzzleObject>(true);
                    for (int a = 0; a < totalAmountPossible.Length; a++)
                    {
                        if (totalAmountPossible[a].gameObject.activeSelf) totalAmountPossible[a].Index = (byte)i;
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
            SetCurrentSelected(_puzzleObjects[_currentPuzzleLayer][0]);
            return PlayerActions.InteractAnimation.Default;
        }

        private void HandleCancelInteraction(InputAction.CallbackContext obj)
        {
            ExitInteraction();
        }

        private void HandleNavigationUI(InputAction.CallbackContext obj)
        {
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
                PlayerActions.Instance.ChangeInputMap("Menu");
            }
            else
            {
                _navigateUIInput.action.performed -= HandleNavigationUI;
                _cancelInteractionInput.action.started -= HandleCancelInteraction;
                PlayerActions.Instance.ChangeInputMap("Player");
            }
        }

        private void SetCurrentSelected(RotatingPuzzleObject selected)
        {
            if (_currentSelected) _currentSelected.InteratctableFeedbacks.UpdateFeedbacks(false, true);
            _currentSelected = selected;
            if (_currentSelected) _currentSelected.InteratctableFeedbacks.UpdateFeedbacks(true, true);
        }

        public void RotateObject(int rotatingObjectIndex)
        {
            if (!_rotationAnimations.ContainsKey(rotatingObjectIndex))
            {
                _rotationAnimations.Add(rotatingObjectIndex, StartCoroutine(RotateCoroutine(rotatingObjectIndex)));
            }
        }

        private IEnumerator RotateCoroutine(int rotatingObjectIndex)
        {
            float count = 0;
            WaitForFixedUpdate delay = new WaitForFixedUpdate();
            float initialAngle = _rotatingObjects[rotatingObjectIndex].eulerAngles.y;
            while (count < 1)
            {
                count += Time.fixedDeltaTime;
                _rotatingObjects[rotatingObjectIndex].Rotate(new Vector3(0, Mathf.LerpAngle(_rotatingObjects[rotatingObjectIndex].eulerAngles.y, initialAngle + _rotationAmount, count), 0), Space.Self);
                yield return delay;
            }
            _rotationAnimations.Remove(rotatingObjectIndex);
            UpdatePuzzleObjectsIndex(rotatingObjectIndex);
            CheckForSolutionsCompleted();
        }

        private void UpdatePuzzleObjectsIndex(int rotatingObjectIndex)
        {
            for (int i = 0; i < _puzzleObjects[rotatingObjectIndex].Length; i++)
            {
                if (_puzzleObjects[rotatingObjectIndex][i].Index + 1 >= _puzzleObjects[rotatingObjectIndex].Length) _puzzleObjects[rotatingObjectIndex][i].Index = 0;
                else _puzzleObjects[rotatingObjectIndex][i].Index++;
            }
        }

        private void CheckForSolutionsCompleted()
        {
            bool foundMatchingPuzzleObjectIndex = false;
            bool puzzleSolutionCompleted = false;
            for (int currentSolutionIndex = 0; currentSolutionIndex < _solutions.Length; currentSolutionIndex++)
            {
                for (int puzzleLayer = 0; puzzleLayer < _solutions[currentSolutionIndex].Solution.Count; puzzleLayer++)
                {
                    for (int puzzleObjectIndexToMatch = 0; puzzleObjectIndexToMatch < _solutions[currentSolutionIndex].Solution[puzzleLayer].Length; puzzleObjectIndexToMatch++)
                    {
                        for (int puzzleObjectIndex = 0; puzzleObjectIndex < _puzzleObjects[puzzleLayer].Length; puzzleObjectIndex++)
                        {
                            foundMatchingPuzzleObjectIndex = false;
                            if (_puzzleObjects[puzzleLayer][puzzleObjectIndex].IsCorrect(_solutions[currentSolutionIndex].Solution[puzzleLayer][puzzleObjectIndexToMatch]))
                            {
                                foundMatchingPuzzleObjectIndex = true;
                                break;
                            }
                        }
                        if (!foundMatchingPuzzleObjectIndex) break;
                    }
                    if (!foundMatchingPuzzleObjectIndex) break;
                    if (puzzleLayer == _solutions[currentSolutionIndex].Solution.Count - 1) puzzleSolutionCompleted = true;
                }
                if (puzzleSolutionCompleted)
                {
                    _solutions[currentSolutionIndex].OnActivate?.Invoke();
                    return;
                }
            }
        }
    }
}