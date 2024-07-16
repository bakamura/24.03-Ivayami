using System.Collections.Generic;
using UnityEngine;
using Ivayami.Save;

namespace Ivayami.Player {
    public class PlayerAnimation : MonoSingleton<PlayerAnimation> {

        [SerializeField] private AnimationClip _enterLockerAnimation;
        public float EnterLockerDuration { get { return _enterLockerAnimation.length; } }

        [Header("Cache")]

        private static int IDLE = Animator.StringToHash("Idle");
        private static int FAIL = Animator.StringToHash("Idle");
        private static int MOVE_SPEED = Animator.StringToHash("MoveSpeed");
        private static int MOVE_X = Animator.StringToHash("MoveX");
        private static int MOVE_Y = Animator.StringToHash("MoveY");
        private static int CROUCH = Animator.StringToHash("Crouch");
        private static Dictionary<PlayerActions.InteractAnimation, int> INTERACT_DICTIONARY = new Dictionary<PlayerActions.InteractAnimation, int> {
            { PlayerActions.InteractAnimation.Default, Animator.StringToHash("Interact") },
            { PlayerActions.InteractAnimation.EnterLocker, Animator.StringToHash("EnterLocker") },
        };
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
            PlayerStress.Instance.onFailState.AddListener(Fail);
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

        private void Interact(PlayerActions.InteractAnimation animation) {
            _animator.SetTrigger(INTERACT_DICTIONARY[animation]);
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

        public void GoToIdle() {
            _animator.SetTrigger(IDLE);
        }

        private void Fail() {
            _animator.SetTrigger(FAIL);
        }

    }
}