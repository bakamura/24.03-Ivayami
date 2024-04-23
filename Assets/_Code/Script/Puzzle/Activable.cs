using UnityEngine;

namespace Ivayami.Puzzle
{
    public class Activable : MonoBehaviour
    {

        [SerializeField] private Activator[] _activators;
        [HideInInspector] public bool IsActive { get; protected set; }

        protected virtual void Awake()
        {
            for (int i = 0; i < _activators.Length; i++)
            {
                _activators[i].onActivate.AddListener(HandleOnActivate);
            }
        }

        protected virtual void HandleOnActivate()
        {
            for (int i = 0; i < _activators.Length; i++)
            {
                if (!_activators[i].IsActive)
                {
                    IsActive = false;
                    return;
                }
            }
            IsActive = true;
        }
    }
}