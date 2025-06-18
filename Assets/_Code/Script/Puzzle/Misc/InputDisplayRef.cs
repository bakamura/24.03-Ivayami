using UnityEngine;
using Ivayami.UI;

namespace Ivayami.Puzzle {
    public class InputDisplayRef : MonoBehaviour {

        [SerializeField] private InputIcons[] _inputIconsDisplayed;

        public void DisplayThis() {
            InputDisplay.Instance.DisplayInputs(_inputIconsDisplayed);
        }

        public void DisplayHide() {
            InputDisplay.Instance.Hide();
        }

    }
}