using Ivayami.Audio;
using UnityEngine;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(Animator), typeof(Activator), typeof(InteractableSounds))]
    public class ActivatorAnimated : MonoBehaviour
    {
        [SerializeField] private bool _autoSubscribeToActivator = true;
        private Animator _animator;
        private Activator _activator;
        private InteractableSounds _interactableSounds;

        private static readonly int ACTIVATE = Animator.StringToHash("activate");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _activator = GetComponent<Activator>();
            _interactableSounds = GetComponent<InteractableSounds>();
            if(_autoSubscribeToActivator)_activator.onActivate.AddListener(HandleOnActivate);
        }

        private void HandleOnActivate()
        {
            Activate(_activator.IsActive);
        }

        public void Activate(bool isActive)
        {
            _animator.SetBool(ACTIVATE, isActive);
            _interactableSounds.PlaySound(isActive ? InteractableSounds.SoundTypes.Activate : InteractableSounds.SoundTypes.Deactivate);
        }
    }
}