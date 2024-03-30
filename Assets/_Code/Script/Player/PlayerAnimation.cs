using UnityEngine;

namespace Paranapiacaba.Player {
    public class PlayerAnimation : MonoBehaviour {

        [Header("Parameter Names")]

        private const string MOVE_SPEED = "MoveSpeed";
        private const string MOVE_X = "MoveX";
        private const string MOVE_Y = "MoveY";
        private const string CROUCH = "Crouch";
        private const string INTERACT_LONG = "InteractLong";

        private Animator _animator;

        private void Awake() {
            _animator = GetComponent<Animator>();
        }

        private void Start() {
            PlayerMovement.Instance.onMovement.AddListener(MoveAnimation);
            PlayerMovement.Instance.onCrouch.AddListener(Crouch);
            PlayerActions.Instance.onInteractLong.AddListener(InteractLong);
        }

        private void MoveAnimation(Vector2 direction) {
            _animator.SetFloat(MOVE_SPEED, direction.magnitude);
            _animator.SetFloat(MOVE_X, direction.x);
            _animator.SetFloat(MOVE_Y, direction.y);
        }

        private void Crouch(bool isCrouching) {
            _animator.SetBool(CROUCH, isCrouching);
        }

        private void InteractLong(bool isInteracting) {
            _animator.SetBool(INTERACT_LONG, isInteracting);
        }

        private void Ability(string abilityName) {
            _animator.SetTrigger(abilityName);
        }

    }
}