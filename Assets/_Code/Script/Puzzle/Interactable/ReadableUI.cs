using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using Ivayami.UI;

namespace Ivayami.Puzzle {
    public class ReadableUI : MonoSingleton<ReadableUI> {

        public Menu Menu { get; private set; }

        [SerializeField] private LocalizeStringEvent _title;
        [SerializeField] private TextMeshProUGUI _content;
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _nextPageBtn;
        [SerializeField] private Button _previousPageBtn;
        private List<TextMeshProUGUI> _contents = new List<TextMeshProUGUI>();
        private int _pageCurrent;
        private int _pageLimitCurrent;

        protected override void Awake() {
            base.Awake();

            Menu = GetComponent<Menu>();
            _contents.Add(_content);
        }

        public void ShowReadable(string title, string content) {
            _title.SetEntry(title);
            int contentIndex = 0;
            int contentBreakPoint;
            string openTag = "";
            while (true) {
                if (_contents.Count <= contentIndex) _contents.Add(Instantiate(_content, _content.transform.parent));
                _contents[contentIndex].text = openTag + content;
                _contents[contentIndex].gameObject.SetActive(true);
                _contents[contentIndex].ForceMeshUpdate();
                _contents[contentIndex].gameObject.SetActive(false);

                contentBreakPoint = _contents[contentIndex].firstOverflowCharacterIndex;
                if (contentBreakPoint != -1) {
                    contentBreakPoint = FindSafeBreakPoint(_contents[contentIndex].text, contentBreakPoint);
                    _contents[contentIndex].text = _contents[contentIndex].text.Substring(0, contentBreakPoint);
                    content = content.Substring(contentBreakPoint);
                    openTag = FindOpenRichText(_contents[contentIndex].text);
                    contentIndex++;
                }
                else {
                    _pageLimitCurrent = contentIndex;
                    break;
                }
            }

            ChangePage(0);
            Menu.Open();
        }

        public int FindSafeBreakPoint(string text, int currentBreakPoint) {
            int newBreakPoint = currentBreakPoint;
            while (newBreakPoint > 0) {
                if (text[newBreakPoint] == '>') return currentBreakPoint;
                if (text[newBreakPoint] == '<') {
                    newBreakPoint--;
                    return newBreakPoint;
                }

                newBreakPoint--;
            }
            return currentBreakPoint;
        }

        public string FindOpenRichText(string text) {
            int openIndex = text.LastIndexOf('<');
            if (openIndex == -1 || text[openIndex + 1] == '/') return "";
            return text.Substring(openIndex).Substring(0, text.LastIndexOf('>'));
        }

        private void ChangePage(int pageToOpen) {
            _contents[_pageCurrent].gameObject.SetActive(false);
            _pageCurrent = pageToOpen;
            _contents[_pageCurrent].gameObject.SetActive(true);
            UpdateButtonStates();
        }

        public void PageNext() {
            if (_pageCurrent < _pageLimitCurrent) ChangePage(_pageCurrent + 1);
        }

        public void PagePrevious() {
            if (_pageCurrent > 0) ChangePage(_pageCurrent - 1);
        }

        private void UpdateButtonStates() {
            _nextPageBtn.gameObject.SetActive(_pageCurrent < _pageLimitCurrent);
            _previousPageBtn.gameObject.SetActive(_pageCurrent > 0);
            if (EventSystem.current.currentSelectedGameObject && !EventSystem.current.currentSelectedGameObject.activeInHierarchy) {
                if (_nextPageBtn.gameObject.activeInHierarchy) _nextPageBtn.Select();
                else if (_previousPageBtn.gameObject.activeInHierarchy) _previousPageBtn.Select();
                else _closeBtn.Select();
            }
        }

    }
}
