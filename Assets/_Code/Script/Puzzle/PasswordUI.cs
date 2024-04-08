using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Paranapiacaba.Puzzle
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PasswordUI : MonoBehaviour
    {        
        [SerializeField] protected string password;
        //[SerializeField] protected UnityEvent onPasswordCorrect;
        //[SerializeField] protected UnityEvent onPasswordWrong;
        [SerializeField] private Button _initialSelectedButton;

        private CanvasGroup _canvasGroup;

        public Button FallbackButton => _initialSelectedButton;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        public abstract bool CheckPassword();

        public virtual void UpdateActiveState(bool isActive)
        {
            _canvasGroup.alpha = isActive ? 1 : 0;
            _canvasGroup.interactable = isActive;
            _canvasGroup.blocksRaycasts = isActive;
            if (isActive) EventSystem.current.SetSelectedGameObject(_initialSelectedButton.gameObject);
        }
    }
}