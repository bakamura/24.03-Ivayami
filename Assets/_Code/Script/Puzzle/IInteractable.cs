
using UnityEngine;

namespace Paranapiacaba.Puzzle {
    public interface IInteractable {

        public GameObject gameObject { get; }

        public abstract bool Interact();

        public virtual void InteractStop() { }

    }
}