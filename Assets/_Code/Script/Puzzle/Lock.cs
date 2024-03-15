using UnityEngine;
using Paranapiacaba.Player;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Paranapiacaba.Puzzle
{
    public class Lock : Activator, IInteractable
    {
        [SerializeField, Tooltip("if empty, will not require it")] private InventoryItem[] _itemsRequired;
        [SerializeField, Tooltip("if null, will not require it")] private string _passwordRequired;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private CanvasGroup _lockUI;
        
        public void Interact()
        {
            _lockUI.alpha = _lockUI.alpha == 1 ? 0 : 1;
            _lockUI.blocksRaycasts = !_lockUI.blocksRaycasts;
            _lockUI.interactable = !_lockUI.interactable;
        }

        public void TryUnlock()
        {
            bool hasItems = true;
            if (_itemsRequired.Length > 0)
            {
                for (int i = 0; i < _itemsRequired.Length; i++)
                {
                    if (!PlayerInventory.Instance.CheckInventoryFor(_itemsRequired[i].id))
                    {
                        hasItems = false;
                        break;
                    }
                }
            }
            onActivate?.Invoke();
        }

    }
}