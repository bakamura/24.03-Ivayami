using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Ivayami.UI
{
    public class Credits : MonoBehaviour
    {
        [SerializeField] private InputActionReference _moveCreditsInput;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _contentTransform;
        [SerializeField, Min(1f)] private float _controlDragSensitivity = 5;

        private Vector2 _currentInputValue;

        public void UpdateInputs(bool isActive)
        {
            if (isActive)
            {
                _moveCreditsInput.action.performed += MovementMap;
                StartCoroutine(MoveMapCoroutine());
            }
            else
            {
                _moveCreditsInput.action.performed -= MovementMap;
                StopCoroutine(MoveMapCoroutine());
            }
        }

        public void ResetCredits()
        {
            _scrollRect.StopMovement();
            _contentTransform.anchoredPosition = Vector2.zero;
        }

        private void MovementMap(InputAction.CallbackContext obj)
        {
            _currentInputValue = obj.ReadValue<Vector2>();
        }

        private IEnumerator MoveMapCoroutine()
        {
            while (true)
            {
                //check value if is greater than controller deadzone
                if (_currentInputValue != Vector2.zero) _contentTransform.anchoredPosition += new Vector2(0, -_currentInputValue.y) * _controlDragSensitivity;
                yield return null;
            }
        }
    }
}