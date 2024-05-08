using UnityEngine;

namespace Ivayami.Puzzle {
    public interface IInteractable {

        public GameObject gameObject { get; }

        public InteractableHighlight InteratctableHighlight { get; }

        public abstract void Interact();

    }
}
