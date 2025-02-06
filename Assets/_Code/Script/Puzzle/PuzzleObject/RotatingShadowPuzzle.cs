using Ivayami.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks))]
    public class RotatingShadowPuzzle : MonoBehaviour, IInteractable
    {
        [SerializeField, Min(0)] private int _rotationAmount = 90;
        [SerializeField, Min(0f)] private float _rotatingDuration = 1;
        [SerializeField] private Transform _rotatingTarget;
        [SerializeField] private Transform[] _possibleShadows;
        [SerializeField] private Transform _startingShadow;
        [SerializeField] private Transform _correctShadow;
        [SerializeField] private UnityEvent _onRotateObjectStart;
        [SerializeField] private UnityEvent _onCorrectShadow;

        public UnityEvent OnRotateObjectEnd;

        private InteractableFeedbacks _interatctableFeedbacks;
        private Coroutine _rotatingCoroutine;
        private Transform _currentShadow;
        private Transform _previousShadow;
        private sbyte _currentShadowIndex = -1;
        private sbyte _correctShadowIndex = -1;
        private bool _hasDoneSetup;
        public InteractableFeedbacks InteratctableFeedbacks
        {
            get
            {
                if (!_interatctableFeedbacks) _interatctableFeedbacks = GetComponent<InteractableFeedbacks>();
                return _interatctableFeedbacks;
            }
        }

        public PlayerActions.InteractAnimation Interact()
        {
            Setup();
            _rotatingCoroutine ??= StartCoroutine(RotateCoroutine());
            return PlayerActions.InteractAnimation.Default;
        }

        private void Setup()
        {
            if (!_hasDoneSetup)
            {
                for(int i = 0; i < _possibleShadows.Length;  i++)
                {
                    if(_possibleShadows[i].GetInstanceID() == _correctShadow.GetInstanceID() && _correctShadowIndex == -1)                    
                        _correctShadowIndex = (sbyte)i;
                    if (_possibleShadows[i].GetInstanceID() == _startingShadow.GetInstanceID() && _currentShadowIndex == -1)
                        _currentShadowIndex = (sbyte)i;
                }
                UpdateShadow(_currentShadowIndex);
                _hasDoneSetup = true;
            }
        }

        private IEnumerator RotateCoroutine()
        {
            float count = 0;
            WaitForFixedUpdate delay = new WaitForFixedUpdate();
            float initialAngle = _rotatingTarget.localEulerAngles.y;
            _onRotateObjectStart?.Invoke();
            while (count < 1)
            {
                count += Time.fixedDeltaTime / _rotatingDuration;
                _rotatingTarget.localEulerAngles = new Vector3(0, Mathf.LerpAngle(initialAngle, initialAngle + _rotationAmount, count), 0);
                yield return delay;
            }
            _currentShadowIndex++;
            LoopValueByArraySize(ref _currentShadowIndex, _possibleShadows.Length);
            UpdateShadow(_currentShadowIndex);
            if (IsCorrect()) _onCorrectShadow?.Invoke();
            OnRotateObjectEnd?.Invoke();
            _rotatingCoroutine = null;
        }

        private void UpdateShadow(sbyte shadowIndex)
        {
            if (_previousShadow) _previousShadow.gameObject.SetActive(false);
            _currentShadow = _possibleShadows[shadowIndex];
            _currentShadow.gameObject.SetActive(true);
            _previousShadow = _currentShadow;
        }

        private void LoopValueByArraySize(ref sbyte valueToConstrain, int arraySize)
        {
            if (valueToConstrain < 0) valueToConstrain = (sbyte)(arraySize - 1);
            else if (valueToConstrain >= arraySize) valueToConstrain = 0;
        }

        public bool IsCorrect()
        {
            return _currentShadowIndex == _correctShadowIndex && _correctShadowIndex != -1;
        }
    }
}