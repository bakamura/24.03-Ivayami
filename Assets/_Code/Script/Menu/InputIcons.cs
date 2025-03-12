using System;
using UnityEngine;
using Ivayami.Player;

namespace Ivayami.UI {
    [CreateAssetMenu(menuName = "InputIcons")]
    public class InputIcons : ScriptableObject {

        [SerializeField] private Sprite[] _icons;
        public Sprite[] Icons { get { return _icons; } }

#if UNITY_EDITOR
        private void OnValidate() {
            int enumSize = Enum.GetNames(typeof(InputCallbacks.ControlType)).Length;
            if (_icons.Length != enumSize) Array.Resize(ref _icons, enumSize);
        }
#endif

    }
}
