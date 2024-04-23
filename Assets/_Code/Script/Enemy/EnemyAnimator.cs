using UnityEngine;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimator : MonoBehaviour
    {
        private static int WALKING = Animator.StringToHash("walking");

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Walking(bool walking)
        {
            _animator.SetBool(WALKING, walking);
        }
    }
}