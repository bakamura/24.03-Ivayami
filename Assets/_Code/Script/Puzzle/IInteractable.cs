using UnityEngine;

namespace Ivayami.Puzzle {
    public interface IInteractable {

        public GameObject gameObject { get; }

        public InteratctableHighlight InteratctableHighlight { get; }

        public abstract void Interact();

    }
}
