using UnityEngine;
using Ivayami.Player.Ability;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(Friend))]
    public class FriendAnimator : MonoBehaviour
    {
        private Animator _animator;
        private static readonly int WALKING = Animator.StringToHash("walking");
        private static readonly int INTERACTING = Animator.StringToHash("interacting");

        public void UpdateWalking(bool walking)
        {
            FindAnimator();
            _animator.SetBool(WALKING, walking);
        }

        public void UpdateInteracting(bool interacting)
        {
            FindAnimator();
            _animator.SetBool(INTERACTING, interacting);
        }

        private void FindAnimator()
        {
            if(!_animator) _animator = GetComponentInChildren<Animator>();
        }
    }
}