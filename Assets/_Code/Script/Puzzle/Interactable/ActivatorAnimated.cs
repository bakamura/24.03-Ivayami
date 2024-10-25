using Ivayami.Audio;
using UnityEngine;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(Animator), typeof(Activator), typeof(InteractableSounds))]
    public class ActivatorAnimated : MonoBehaviour
    {
        private Animator _animator
        {
            get
            {
                if (!m_animator) m_animator = GetComponent<Animator>();
                return m_animator;
            }
        }
        private Animator m_animator;
        private Activator _activator
        {
            get
            {
                if (!m_activator) m_activator = GetComponent<Activator>();
                return m_activator;
            }
        }
        private Activator m_activator;
        private InteractableSounds _interactableSounds
        {
            get
            {
                if (!m_interactableSounds) m_interactableSounds = GetComponent<InteractableSounds>();
                return m_interactableSounds;
            }           
        }
        private InteractableSounds m_interactableSounds;

        private static readonly int ACTIVATE = Animator.StringToHash("activate");

        private void Awake()
        {
            _activator.onActivate.AddListener(HandleOnActivate);
        }

        private void HandleOnActivate()
        {
            _animator.SetBool(ACTIVATE, _activator.IsActive);
            _interactableSounds.PlaySound(_activator.IsActive ? InteractableSounds.SoundTypes.Activate : InteractableSounds.SoundTypes.Deactivate);
        }

        //public void UpdateAnimation(bool isActive)
        //{
        //    _animator.SetBool(ACTIVATE, isActive);
        //    _interactableSounds.PlaySound(isActive ? InteractableSounds.SoundTypes.Activate : InteractableSounds.SoundTypes.Deactivate);
        //}
    }
}