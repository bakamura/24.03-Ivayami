using System.Collections;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace Ivayami.Puzzle
{
    public class RotateLock : PasswordUI
    {
        [SerializeField, Min(.02f)] private float _animationDuration;
        [SerializeField, Range(0f, 360f)] private float _rotationAngle = 60;
        //[SerializeField] private Transform _lockBtnContainer;

        private RotateLockButton[] _buttons;
        private Coroutine _animateCoroutine;
        private RotateLockButton _currentBtn;

        protected override void Awake()
        {
            base.Awake();
            _buttons = GetComponentsInChildren<RotateLockButton>(true);
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
                //Cursor.lockState = CursorLockMode.Locked;
                navegationUIInput.action.performed += HandleNavigateUI;
                _lock.InteratctableHighlight.UpdateFeedbacks(false, true);
                _currentBtn.InteratctableHighlight.UpdateFeedbacks(true);
            }
            else
            {
                //Cursor.lockState = CursorLockMode.None;
                navegationUIInput.action.performed -= HandleNavigateUI;
                _currentBtn.InteratctableHighlight.UpdateFeedbacks(false);
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

        protected override void HandleNavigateUI(InputAction.CallbackContext obj)
        {
            base.HandleNavigateUI(obj);
            Vector2 input = obj.ReadValue<Vector2>();
            if (input.y != 0 && _animateCoroutine == null)
            {
                _lock.LockSounds.PlaySound(Audio.LockPuzzleSounds.SoundTypes.ConfirmOption);
                _currentBtn.UpdateButtonDisplay((sbyte)Mathf.Sign(input.y));
                _animateCoroutine = StartCoroutine(AnimateButtonCoroutine((sbyte)Mathf.Sign(input.y), _currentBtn.ButtonVisualTransform));
            }
        }

        private IEnumerator AnimateButtonCoroutine(sbyte direction, Transform rotateObject)
        {
            float count = 0;
            RotateLockButton currentBtn = _currentBtn;
            Quaternion initialRotation = rotateObject.localRotation;
            Quaternion finalRotation = Quaternion.Euler(initialRotation.eulerAngles.x + _rotationAngle * direction, 0, 0);
            while (count < 1)
            {
                count += Time.deltaTime / _animationDuration;
                rotateObject.localRotation = Quaternion.Lerp(initialRotation, finalRotation, count);
                yield return null;
            }
            currentBtn.UpdateButtonDisplay(0);
            rotateObject.localRotation = initialRotation;
            _animateCoroutine = null;
            //_onCheckPassword?.Invoke();
        }

        public void SetCurrentSelected(RotateLockButton btn)
        {
            if (_currentBtn) _currentBtn.InteratctableHighlight.UpdateFeedbacks(false);
            _currentBtn = btn;
            _currentBtn.InteratctableHighlight.UpdateFeedbacks(true);
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