using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ivayami.UI;

namespace Ivayami.Puzzle {
    public class ReadableUI : MonoSingleton<ReadableUI> {

        public Menu Menu { get; private set; }

        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _content;
        private List<TextMeshProUGUI> _contents = new List<TextMeshProUGUI>();
        private int _pageCurrent;

        protected override void Awake() {
            base.Awake();

            Menu = GetComponent<Menu>();
            _contents.Add(_content);
        }

        public void ShowReadable(string title, string content) {
            StartCoroutine(ShowReadableRoutine(title, content));
        }

        private IEnumerator ShowReadableRoutine(string title, string content) {
            _title.text = title;
            int contentIndex = 0;
            int overflowIndex;
            while (content.Length > 0) {
                if (_contents.Count <= contentIndex) _contents.Add(Instantiate(_content, _content.transform.parent));
                _contents[contentIndex].text = content;

                yield return null;

                overflowIndex = _contents[contentIndex].firstOverflowCharacterIndex;
                content = overflowIndex > 0 ? content.Substring(overflowIndex) : string.Empty;
                if (content.Length > 0) {
                    _contents[contentIndex].text = _contents[contentIndex].text.Substring(0, overflowIndex);
                    contentIndex++;
                }
            }

            ChangePage(0);
            Menu.Open();
        }

        private void ChangePage(int pageToOpen) {
            _contents[_pageCurrent].gameObject.SetActive(false);
            _pageCurrent = pageToOpen;
            _contents[_pageCurrent].gameObject.SetActive(true);
        }

        public void PageNext() {
            if (_pageCurrent + 1 < _contents.Count) ChangePage(_pageCurrent + 1);
        }

        public void PagePrevious() {
            if (_pageCurrent > 0) ChangePage(_pageCurrent - 1);
        }

    }
}
