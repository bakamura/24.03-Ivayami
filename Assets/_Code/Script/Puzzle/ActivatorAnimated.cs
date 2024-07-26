using Ivayami.Audio;
using UnityEngine;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(Animator), typeof(Activator), typeof(InteractableSounds))]
    public class ActivatorAnimated : MonoBehaviour
    {
        private Animator _animator;
        private Activator _activator;
        private InteractableSounds _interactableSounds;

        private static readonly int ACTIVATE = Animator.StringToHash("activate");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _activator = GetComponent<Activator>();
            _interactableSounds = GetComponent<InteractableSounds>();
            _activator.onActivate.AddListener(HandleOnActivate);
        }

        private void HandleOnActivate()
        {
            _animator.SetBool(ACTIVATE, _activator.IsActive);
            _interactableSounds.PlaySound(_activator.IsActive ? InteractableSounds.SoundTypes.Activate : InteractableSounds.SoundTypes.Deactivate);
        }
    }
}