using Ivayami.Player;
using UnityEngine;

namespace Ivayami.Puzzle {    
    public interface IInteractable {

        public GameObject gameObject { get; }

        public InteractableFeedbacks InteratctableFeedbacks { get; }

        public abstract PlayerActions.InteractAnimation Interact();

    }
}
