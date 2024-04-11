using UnityEngine;
using UnityEngine.Events;

namespace Paranapiacaba.Puzzle {
    public abstract class Activator : MonoBehaviour {

        public UnityEvent onActivate;

        public bool IsActive {  get; protected set; }

    }
}