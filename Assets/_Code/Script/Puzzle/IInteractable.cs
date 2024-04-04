
using UnityEngine;

namespace Paranapiacaba.Puzzle {
    public interface IInteractable {

        public GameObject gameObject { get; }

        public abstract void Interact();

        //public abstract void InteractStop();

    }
}