using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Ivayami.Puzzle
{
    //[RequireComponent(typeof(CanvasGroup))]
    public abstract class PasswordUI : MonoBehaviour
    {        
        [SerializeField] protected string password;
        [SerializeField] protected UnityEvent _onCheckPassword;
        //[SerializeField] protected UnityEvent onPasswordCorrect;
        //[SerializeField] protected UnityEvent onPasswordWrong;
        [SerializeField] private GameObject _initialSelected;

        private CanvasGroup _canvasGroup;
        //private Transform _container;

        public GameObject FallbackButton => _initialSelected;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            //if (!_canvasGroup) _container = transform.Find("Container");
        }
        public abstract bool CheckPassword();

        public virtual void UpdateActiveState(bool isActive)
        {
            if (_canvasGroup)
            {
                _canvasGroup.alpha = isActive ? 1 : 0;
                _canvasGroup.interactable = isActive;
                _canvasGroup.blocksRaycasts = isActive;
            }
            //else
            //{
            //    _container.gameObject.SetActive(isActive);
            //}
            if (isActive) EventSystem.current.SetSelectedGameObject(_initialSelected);
        }
    }
}