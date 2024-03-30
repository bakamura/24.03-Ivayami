using UnityEngine;

namespace Paranapiacaba.Puzzle {
    public class Activable : MonoBehaviour {

        [SerializeField] private Activator[] _activators;
        [HideInInspector] public bool IsActive;

        protected virtual void Awake()
        {
            for(int i = 0; i < _activators.Length; i++)
            {
                _activators[i].onActivate.AddListener(HandleOnActivate);
            }
        }

        protected virtual void HandleOnActivate()
        {
            IsActive = !IsActive;
        }
    }
}