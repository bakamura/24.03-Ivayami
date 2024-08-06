using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PasswordUI : MonoBehaviour
    {
        [SerializeField] protected InputActionReference navegationUIInput;
        [SerializeField] protected string password;
        [HideInInspector] public Action OnCheckPassword;
        //[SerializeField] protected UnityEvent onPasswordCorrect;
        //[SerializeField] protected UnityEvent onPasswordWrong;
        [SerializeField] private Selectable _initialSelected;

        private CanvasGroup _canvasGroup;
        protected Lock _lock;
        //private Transform _container;

        //public Selectable FallbackButton => _initialSelected;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _lock = GetComponentInParent<Lock>();
            //_container = transform.Find("Container");
        }
        public abstract bool CheckPassword();

        public virtual void UpdateActiveState(bool isActive)
        {
            //if (_canvasGroup)
            //{
            _canvasGroup.alpha = isActive ? 1 : 0;
            _canvasGroup.interactable = isActive;
            _canvasGroup.blocksRaycasts = isActive;
            //_container.gameObject.SetActive(isActive);
            //}
            //else
            //{
            //    _container.gameObject.SetActive(isActive);
            //}
            if (isActive)
            {
                _initialSelected.Select();//EventSystem.current.SetSelectedGameObject(_initialSelected);
                navegationUIInput.action.performed += HandleNavigateUI;
            }
            else
            {
                navegationUIInput.action.performed -= HandleNavigateUI;
            }
        }

        protected virtual void HandleNavigateUI(InputAction.CallbackContext obj)
        {
            if(obj.ReadValue<Vector2>() != Vector2.zero && !EventSystem.current.currentSelectedGameObject) _initialSelected.Select();
        }
    }
}