using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using Ivayami.UI;

namespace Ivayami.Puzzle {
    public class ReadableUI : MonoSingleton<ReadableUI> {

        public Menu Menu { get; private set; }

        [SerializeField] private LocalizeStringEvent _title;
        [SerializeField] private RectTransform _contentsParent;

        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _nextPageBtn;
        [SerializeField] private Button _previousPageBtn;

        private List<PagePreset> _contents = new List<PagePreset>();
        private int _pageCurrent;
        private int _pageLimitCurrent;

        protected override void Awake() {
            base.Awake();

            Menu = GetComponent<Menu>();
        }

        public void ShowReadable(Readable readable) {
            _title.SetEntry(readable.TitleLocalizationString);

            foreach (PagePreset preset in _contents) Destroy(preset.gameObject);
            _contents.Clear();
            int textsInserted = 0;
            for (int i = 0; i < readable.PagePresets.Length; i++) {
                _contents.Add(Instantiate(readable.PagePresets[i], _contentsParent));
                _contents[i].DisplayContent(readable.GetTextBoxes(i), readable.GetPageSprites(i));
                _contents[i].gameObject.SetActive(false);
                textsInserted += readable.PagePresets[i].TextBoxAmount;
            }
            _pageLimitCurrent = readable.PagePresets.Length - 1;
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
