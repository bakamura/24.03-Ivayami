using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ivayami.UI {
    [CreateAssetMenu(menuName = "Texts/UI")]
    public class UiText : ScriptableObject {

        [SerializeField] private string[] _keys;
        [SerializeField] private string[] _values;
        private Dictionary<string, string> _dictionary;
        public int Size { get { return _keys.Length; } }
        public string[] Keys { get { return _keys; } }

        public string GetText(string key) {
            if(_dictionary == null) InitDict();
            return _dictionary[key];
        }

        private void InitDict() {
            if (_keys != null && _keys.Length > 0) {
                _dictionary = new Dictionary<string, string>();
                for (int i = 0; i < _keys.Length; i++) _dictionary.Add(_keys[i], _values[i]);
#if !UNITY_EDITOR
                _keys = null;
                _values = null;
#endif
            }
            else {
                Debug.LogError($"Could not initialize UiText with null Key/Values '{name}'");
            }
        }

        public UiText GetTranslation(LanguageTypes language) {
            if (language == LanguageTypes.ENUS) return this;
            UiText uiText = Resources.LoadAll<UiText>($"UiText/{language}").First(text => text.name == name);
            Resources.UnloadUnusedAssets();
            if (uiText != null) return uiText;
            else {
                Debug.LogError($"No translation {language} found of '{name}' (UiText)");
                return this;
            }
        }

    }
}