using UnityEngine;
using UnityEngine.UI;

namespace Ivayami.Puzzle
{
    public class FuseBoxButtonUI : MonoBehaviour
    {
        public int ButtonIndex { get; private set; }
        public Button Button
        {
            get
            {
                if (!_button) _button = GetComponent<Button>();
                return _button;
            }
        }
        private Button _button;

        public void Setup(int buttonIndex)
        {
            ButtonIndex = buttonIndex;
        }
    }

}
