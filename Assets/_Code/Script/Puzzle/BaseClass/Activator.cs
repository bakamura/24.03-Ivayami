using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Puzzle {
    public abstract class Activator : MonoBehaviour {

        public UnityEvent onActivate;

        public bool IsActive {  get; protected set; }

    }
}