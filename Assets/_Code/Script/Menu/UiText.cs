using System.Collections.Generic;
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
                _keys = null;
                _values = null;
            }
            else {
                Debug.LogError($"Could not initialize UiText with null Key/Values '{name}'");
            }
        }

    }
}