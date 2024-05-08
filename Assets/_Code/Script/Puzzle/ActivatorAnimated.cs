using UnityEngine;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(Animator), typeof(Activator))]
    public class ActivatorAnimated : MonoBehaviour
    {
        private Animator _animator;
        private Activator _activator;

        private static readonly int ACTIVATE = Animator.StringToHash("activate");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _activator = GetComponent<Activator>();
            _activator.onActivate.AddListener(HandleOnActivate);
        }        

        private void HandleOnActivate()
        {
            _animator.SetBool(ACTIVATE, _activator.IsActive);
        }
    }
}