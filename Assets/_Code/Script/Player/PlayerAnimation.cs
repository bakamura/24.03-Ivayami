using Ivayami.Save;
using System.Collections.Generic;
using UnityEngine;

namespace Ivayami.Player {
    public class PlayerAnimation : MonoSingleton<PlayerAnimation> {

        [Header("Parameter Names")]

        private static int MOVE_SPEED = Animator.StringToHash("MoveSpeed");
        private static int MOVE_X = Animator.StringToHash("MoveX");
        private static int MOVE_Y = Animator.StringToHash("MoveY");
        private static int CROUCH = Animator.StringToHash("Crouch");
        private static Dictionary<PlayerActions.InteractAnimation, int> INTERACT_DICTIONARY = new Dictionary<PlayerActions.InteractAnimation, int> {
            { PlayerActions.InteractAnimation.Default, Animator.StringToHash("Interact") },
            { PlayerActions.InteractAnimation.EnterWardrobe, Animator.StringToHash("EnterWardrobe") },

        }
        private static int INTERACT_LONG = Animator.StringToHash("InteractLong");
        private static int HOLDING = Animator.StringToHash("Holding");

        private Animator _animator;

        protected override void Awake() {
            base.Awake();

            _animator = GetComponent<Animator>();
        }

        private void Start() {
            PlayerMovement.Instance.onMovement.AddListener(MoveAnimation);
            PlayerMovement.Instance.onCrouch.AddListener(Crouch);
            PlayerActions.Instance.onInteract.AddListener(Interact);
            PlayerActions.Instance.onInteractLong.AddListener(InteractLong);
            PlayerActions.Instance.onAbility.AddListener(Trigger);
            SavePoint.onSaveGame.AddListener(() => Trigger("Seat"));
        }

        private void MoveAnimation(Vector2 direction) {
            _animator.SetFloat(MOVE_SPEED, direction.magnitude);
            _animator.SetFloat(MOVE_X, direction.x);
            _animator.SetFloat(MOVE_Y, direction.y);
        }

        private void Crouch(bool isCrouching) {
            _animator.SetBool(CROUCH, isCrouching);
        }

        private void Interact() {
            _animator.SetTrigger(INTERACT);
        }

        private void InteractLong(bool isInteracting) {
            _animator.SetBool(INTERACT_LONG, isInteracting);
        }

        public void Hold(bool isHolding) {
            _animator.SetBool(HOLDING, isHolding);
        }

        private void Trigger(string abilityName) {
            _animator.SetTrigger(abilityName);
        }

    }
}