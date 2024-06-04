using UnityEngine;

namespace Ivayami.Puzzle {    
    public interface IInteractable {

        public GameObject gameObject { get; }

        public InteractableFeedbacks InteratctableHighlight { get; }

        public abstract void Interact();

    }
}
