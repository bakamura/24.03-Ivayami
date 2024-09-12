using Ivayami.Player;
using UnityEngine;

namespace Ivayami.Puzzle {    
    public interface IInteractable {

        public GameObject gameObject { get; }

        public InteractableFeedbacks InteratctableHighlight { get; }

        public abstract PlayerActions.InteractAnimation Interact();

    }
}
