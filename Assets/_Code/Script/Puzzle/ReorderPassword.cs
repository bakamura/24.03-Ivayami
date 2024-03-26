using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Paranapiacaba.Puzzle
{
    public class ReorderPassword : PasswordUI
    {
        [SerializeField] private RectTransform _buttonsContainer;
        [SerializeField] private Color _selectedButtonColor = Color.red;
        [SerializeField] private UnityEvent _onCheckPassword;

        private Button _currentChosenBtn;

        public override bool CheckPassword()
        {
            string temp = "";
            foreach(TMP_Text text in _buttonsContainer.GetComponentsInChildren<TMP_Text>())
            {
                temp += text.text;
            }
            return string.Equals(temp, password);
        }

        public void UpdateSelectedBtn()
        {
            Button btn = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            Image btnImage = btn.GetComponent<Image>();
            if (btn == _currentChosenBtn)
            {
                btnImage.color = Color.white;
                _currentChosenBtn = null;
            }
            else if (_currentChosenBtn == null)
            {
                btnImage.color = _selectedButtonColor;
                _currentChosenBtn = btn;
            }
            else if(btn != _currentChosenBtn && _currentChosenBtn != null)
            {
                _currentChosenBtn.GetComponent<Image>().color = Color.white;
                btnImage.color = Color.white;
                int currentChosenIndex = _currentChosenBtn.transform.GetSiblingIndex();
                _currentChosenBtn.transform.SetSiblingIndex(btn.transform.GetSiblingIndex());
                btn.transform.SetSiblingIndex(currentChosenIndex);
                _currentChosenBtn = null;
                _onCheckPassword?.Invoke();
            }
        }
    }
}