using System.Collections;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Ivayami.Puzzle
{
    public class RotateLock : PasswordUI
    {
        [SerializeField] private InputActionReference _navegationUIInput;
        [SerializeField, Min(.02f)] private float _animationDuration;
        [SerializeField, Range(0f, 360f)] private float _rotationAngle = 60;
        [SerializeField] private Transform _lockBtnContainer;

        private RotateLockButton[] _buttons;
        private sbyte _currentBtnSelectedIndex;
        private Coroutine _animateCoroutine;
        private RotateLockButton _previousBtn;
        private Lock _lock;

        protected override void Awake()
        {
            base.Awake();
            _buttons = _lockBtnContainer.GetComponentsInChildren<RotateLockButton>(true);
            _lock = GetComponentInParent<Lock>();
        }

        public override void UpdateActiveState(bool isActive)
        {
            base.UpdateActiveState(isActive);
            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].Button.interactable = isActive;
            }
            if (isActive)
            {
                Cursor.lockState = CursorLockMode.Locked;
                _navegationUIInput.action.performed += HandleNavigateUI;
                _lock.InteratctableHighlight.UpdateFeedbacks(false);
                _buttons[_currentBtnSelectedIndex].InteratctableHighlight.UpdateFeedbacks(true);
                _previousBtn = _buttons[_currentBtnSelectedIndex];
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                _navegationUIInput.action.performed -= HandleNavigateUI;
                _buttons[_currentBtnSelectedIndex].InteratctableHighlight.UpdateFeedbacks(false);
            }            
        }

        public override bool CheckPassword()
        {
            string currentPassword = null;
            for (int i = 0; i < _buttons.Length; i++)
            {
                currentPassword += _buttons[i].GetCurrentDisplayValue();
            }
            return string.Equals(currentPassword, password);
        }

        private void HandleNavigateUI(InputAction.CallbackContext context)
        {
            //update index
            Vector2 input = context.ReadValue<Vector2>();
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                _currentBtnSelectedIndex += (sbyte)input.x;
                if (_currentBtnSelectedIndex >= _buttons.Length) _currentBtnSelectedIndex = 0;
                else if (_currentBtnSelectedIndex < 0) _currentBtnSelectedIndex = (sbyte)(_buttons.Length - 1);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(FallbackButton);
                _currentBtnSelectedIndex = 0;
            }
            //update selected visual
            if (_previousBtn) _previousBtn.InteratctableHighlight.UpdateFeedbacks(false);
            _buttons[_currentBtnSelectedIndex].InteratctableHighlight.UpdateFeedbacks(true);
            _previousBtn = _buttons[_currentBtnSelectedIndex];

            if (input.y != 0 && _animateCoroutine == null)
            {
                _buttons[_currentBtnSelectedIndex].UpdateButtonDisplay((sbyte)Mathf.Sign(input.y));
                _animateCoroutine = StartCoroutine(AnimateButtonCoroutine((sbyte)Mathf.Sign(input.y), _buttons[_currentBtnSelectedIndex].transform));
            }
        }

        private IEnumerator AnimateButtonCoroutine(sbyte direction, Transform rotateObject)
        {
            _navegationUIInput.action.Disable();
            float count = 0;
            Quaternion initialRotation = rotateObject.localRotation;
            Quaternion finalRotation = Quaternion.Euler(initialRotation.eulerAngles.x + _rotationAngle * direction, 0, 0);
            while (count < 1)
            {
                count += Time.deltaTime / _animationDuration;
                rotateObject.localRotation = Quaternion.Lerp(initialRotation, finalRotation, count);
                yield return null;
            }
            _buttons[_currentBtnSelectedIndex].UpdateButtonDisplay(0);
            rotateObject.localRotation = initialRotation;
            _animateCoroutine = null;
            _navegationUIInput.action.Enable();
            //_onCheckPassword?.Invoke();
        }

        //private void OnValidate()
        //{
        //    foreach (RectTransform rect in GetComponentsInChildren<RectTransform>())
        //    {
        //        if (rect.rotation.x != 0) rect.rotation = Quaternion.Euler(Mathf.Sign(rect.rotation.x) * _rotationAngle, 0, 0);
        //    }
        //}
    }
}