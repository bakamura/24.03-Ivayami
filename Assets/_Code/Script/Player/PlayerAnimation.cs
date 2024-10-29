using System.Collections.Generic;
using UnityEngine;
using Ivayami.Save;

namespace Ivayami.Player {
    public class PlayerAnimation : MonoSingleton<PlayerAnimation> {

        [SerializeField, Range(0, 1), Tooltip("Percentage of Stamina to start animation")] private float _startTiredAnimThreshold = .1f;
        [SerializeField] private AnimationClip[] _interactAnimations;
        private Dictionary<PlayerActions.InteractAnimation, float> _interactAnimationDuration = new Dictionary<PlayerActions.InteractAnimation, float>();

        [Header("Cache")]

        private static int IDLE = Animator.StringToHash("Idle");
        private static int FAIL = Animator.StringToHash("Fail");
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
        private static int GETUP = Animator.StringToHash("GetUp");

        private Animator _animator;

        protected override void Awake() {
            base.Awake();

            _animator = GetComponent<Animator>();
            for (int i = 0; i < _interactAnimations.Length; i++) _interactAnimationDuration.Add((PlayerActions.InteractAnimation) i, _interactAnimations[i].length);
        }

        private void Start() {
            PlayerMovement.Instance.onMovement.AddListener(MoveAnimation);
            PlayerMovement.Instance.onCrouch.AddListener(Crouch);
            PlayerMovement.Instance.onStaminaUpdate.AddListener(Stamina);
            PlayerStress.Instance.onFail.AddListener(Fail);
            PlayerActions.Instance.onInteract.AddListener(Interact);
            PlayerActions.Instance.onInteractLong.AddListener(InteractLong);
            PlayerActions.Instance.onAbility.AddListener(Trigger);
            SavePoint.onSaveGame.AddListener(() => Trigger("Seat"));
        }

        public float GetInteractAnimationDuration(PlayerActions.InteractAnimation animation) {
            return _interactAnimationDuration[animation];
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
            if (INTERACT_DICTIONARY.ContainsKey(animation))
                _animator.SetTrigger(INTERACT_DICTIONARY[animation]);            
            else _animator.SetTrigger(INTERACT_DICTIONARY[PlayerActions.InteractAnimation.Default]);
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

        public void GetUp() {
            _animator.SetTrigger(GETUP);
        }

        private void Fail() {
            _animator.SetTrigger(FAIL);
        }

        private void Stamina(float currentValue){
            if(currentValue <= _startTiredAnimThreshold)
            {
                _animator.SetLayerWeight(2, 1 - (currentValue / _startTiredAnimThreshold));
            }
            else _animator.SetLayerWeight(2, 0);
        }

    }
}