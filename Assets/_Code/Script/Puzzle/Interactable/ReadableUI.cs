using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ivayami.UI;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;

namespace Ivayami.Puzzle
{
    public class ReadableUI : MonoSingleton<ReadableUI>
    {

        public Menu Menu { get; private set; }

        [SerializeField] private LocalizeStringEvent _title;
        [SerializeField] private TextMeshProUGUI _content;
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _nextPageBtn;
        [SerializeField] private Button _previousPageBtn;
        private List<TextMeshProUGUI> _contents = new List<TextMeshProUGUI>();
        private int _pageCurrent;

        protected override void Awake()
        {
            base.Awake();

            Menu = GetComponent<Menu>();
            _contents.Add(_content);
        }

        public void ShowReadable(string title, string content)
        {
            //ShowReadableRoutine(title, content);
            StartCoroutine(ShowReadableRoutine(title, content));
        }

        private IEnumerator ShowReadableRoutine(string title, string content)
        {
            _title.SetEntry(title);
            int contentIndex = 0;
            int overflowIndex;
            while (content.Length > 0)
            {
                if (_contents.Count <= contentIndex) _contents.Add(Instantiate(_content, _content.transform.parent));
                _contents[contentIndex].text = content;

                yield return null;

                overflowIndex = _contents[contentIndex].firstOverflowCharacterIndex;
                content = overflowIndex > 0 ? content.Substring(overflowIndex) : string.Empty;
                if (content.Length > 0)
                {
                    _contents[contentIndex].text = _contents[contentIndex].text.Substring(0, overflowIndex);
                    contentIndex++;
                }
            }

            for(int i = 0; i < _contents.Count; i++)
            {
                _contents[i].gameObject.SetActive(false);
            }

            ChangePage(0);
            UpdateButtonStates();
            Menu.Open();
        }

        private void ChangePage(int pageToOpen)
        {
            _contents[_pageCurrent].gameObject.SetActive(false);
            _pageCurrent = pageToOpen;
            _contents[_pageCurrent].gameObject.SetActive(true);
            UpdateButtonStates();
        }

        public void PageNext()
        {
            if (_pageCurrent + 1 < _contents.Count) ChangePage(_pageCurrent + 1);
        }

        public void PagePrevious()
        {
            if (_pageCurrent > 0) ChangePage(_pageCurrent - 1);
        }

        private void UpdateButtonStates()
        {
            _nextPageBtn.gameObject.SetActive(_pageCurrent + 1 < _contents.Count);
            _previousPageBtn.gameObject.SetActive(_pageCurrent > 0);
            if (EventSystem.current.currentSelectedGameObject && !EventSystem.current.currentSelectedGameObject.activeInHierarchy)
            {
                if (_nextPageBtn.gameObject.activeInHierarchy) _nextPageBtn.Select();
                else if (_previousPageBtn.gameObject.activeInHierarchy) _previousPageBtn.Select();
                else _closeBtn.Select();
            }
        }
    }
}
