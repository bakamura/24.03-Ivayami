using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Default {
    public class HashKeyBlocker {

        private HashSet<string> _blockers = new HashSet<string>();
        
        public UnityEvent<bool> OnToggleChange = new UnityEvent<bool>();
        public UnityEvent OnAllow = new UnityEvent();
        public UnityEvent OnBlock = new UnityEvent();

        public bool IsAllowed => _blockers.Count < 1;

        public HashKeyBlocker() {
            OnToggleChange.AddListener(shouldAllow => (shouldAllow ? OnAllow : OnBlock).Invoke());
        }

        public void Toggle(string key, bool shouldAllow) {
            bool hadChange = shouldAllow ? _blockers.Remove(key) : _blockers.Add(key);
            if (hadChange) OnToggleChange.Invoke(_blockers.Count < 1);
            else Debug.LogWarning($"'{key}' tried to {(shouldAllow ? "un" : "")}block but was already {(shouldAllow ? "not" : "")} blocking");
        }

    }
}
