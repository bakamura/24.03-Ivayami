using UnityEngine;
using Ivayami.Player.Ability;
using Ivayami.Player;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(Friend))]
    public class FriendAnimator : MonoBehaviour
    {
        private Animator _animator
        {
            get
            {
                if (!TryGetComponent<Animator>(out m_animator)) m_animator = GetComponentInChildren<Animator>();
                return m_animator;
            }
        }
        private Animator m_animator;
        private static readonly int VELOCITY_FLOAT = Animator.StringToHash("velocity");
        private static readonly int INTERACT_BUTTON_BOOL = Animator.StringToHash("interactButton");
        private static readonly int INTERACT_ROPE_BOOL = Animator.StringToHash("interactRope");
        private static readonly int INTERACT_LEVER_BOOL = Animator.StringToHash("interactLever");
        private static readonly int DEFAULT_INTERACT_BOOL = Animator.StringToHash("defaultInteract");

        public void UpdateWalking(float velocity)
        {
            _animator.SetFloat(VELOCITY_FLOAT, velocity);
        }

        public void UpdateInteraction(PlayerActions.InteractAnimation interactAnimation, bool interacting)
        {
            switch (interactAnimation)
            {
                case PlayerActions.InteractAnimation.PullRope:
                    _animator.SetBool(INTERACT_ROPE_BOOL, interacting);
                    break;
                case PlayerActions.InteractAnimation.PullLever:
                    _animator.SetBool(INTERACT_LEVER_BOOL, interacting);
                    break;
                case PlayerActions.InteractAnimation.PushButton:
                    _animator.SetBool(INTERACT_BUTTON_BOOL, interacting);
                    break;
                default:
                    _animator.SetBool(DEFAULT_INTERACT_BOOL, interacting);
                    break;
            }
        }
    }
}