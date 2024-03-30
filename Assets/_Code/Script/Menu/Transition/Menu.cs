using UnityEngine;

namespace Paranapiacaba.UI {
    public abstract class Menu : MonoBehaviour {

        [SerializeField] private AnimationCurve _transitionCurve;

        public abstract void Open();

        public abstract void Close();

    }
}