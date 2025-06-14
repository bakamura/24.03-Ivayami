using System.Collections.Generic;
using UnityEngine;

namespace Ivayami.Player
{
    public class PlayerAnimation : MonoSingleton<PlayerAnimation>
    {

        [SerializeField, Range(0, 1), Tooltip("Percentage of Stamina to start animation")] private float _startTiredAnimThreshold = .1f;
        [SerializeField, Min(0)] private int _staminaAnimationLayer = 1;
        //[SerializeField, Min(0)] private int _damageMentalLayer = 2;
        //[SerializeField, Min(0)] private int _damagePhysicalLayer = 3;
        [SerializeField] private AnimationInfo[] _interactAnimations;
        private Dictionary<PlayerActions.InteractAnimation, AnimationInfo> _interactAnimationDuration = new Dictionary<PlayerActions.InteractAnimation, AnimationInfo>();

        [Header("Cache")]

        private static int IDLE = Animator.StringToHash("Idle");
        private static int FAIL = Animator.StringToHash("Fail");
        private static int MOVE_SPEED = Animator.StringToHash("MoveSpeed");
        private static int MOVE_SPEED_MULTIPLIER = Animator.StringToHash("MoveSpeedMultiplier");
        //private static int MOVE_X = Animator.StringToHash("MoveX");
        //private static int MOVE_Y = Animator.StringToHash("MoveY");
        private static int RUN = Animator.StringToHash("Run");
        private static int CROUCH = Animator.StringToHash("Crouch");
        private static int INTERACT = Animator.StringToHash("Interact");
        private static int STRESS_CURRENT = Animator.StringToHash("StressCurrent");
        private static int INTERACT_LONG = Animator.StringToHash("InteractLong");
        private static int HOLDING = Animator.StringToHash("Holding");
        private static int HEAVY_HOLDING = Animator.StringToHash("HeavyHolding");
        private static int GETUP = Animator.StringToHash("GetUp");
        private static int GETUP_SIT = Animator.StringToHash("GetUpSit");
        private static int SIT = Animator.StringToHash("Sit");
        private static int USEMP3 = Animator.StringToHash("UseMP3");
        private static int INTERACT_INDEX = Animator.StringToHash("InteractIndex");
        private static int INTERACT_SPEED = Animator.StringToHash("InteractSpeed");
        //private static int TAKE_DAMAGE = Animator.StringToHash("Damage");
        private static int DAMAGE_TYPE = Animator.StringToHash("DamageType");

        private Animator _animator;
        [System.Serializable]
        private struct AnimationInfo
        {
            public PlayerActions.InteractAnimation InteractType;
            public AnimationClip Animation;
            public float Speed;

            public float GetAnimationDuration()
            {
                if (Animation != null) return Animation.length / Speed;
                Debug.LogError($"Player Animation '{InteractType}' has no AnimationClip assigned!");
                return 1;
            }
        }
        public enum DamageAnimation
        {
            None,
            Mental,
            Physical
        }

        protected override void Awake()
        {
            base.Awake();

            _animator = GetComponent<Animator>();
            for (int i = 0; i < _interactAnimations.Length; i++)
            {
                if (!_interactAnimationDuration.ContainsKey(_interactAnimations[i].InteractType)) _interactAnimationDuration.Add(_interactAnimations[i].InteractType, _interactAnimations[i]);
                else Debug.LogWarning($"The animation {_interactAnimations[i].InteractType} already has an entry in the list");
            }
        }

        private void Start()
        {
            PlayerMovement.Instance.onMovement.AddListener(MoveAnimation);
            PlayerMovement.Instance.onRunToggle.AddListener(Run);
            PlayerMovement.Instance.onCrouch.AddListener(Crouch);
            PlayerMovement.Instance.onStaminaUpdate.AddListener(Stamina);
            PlayerStress.Instance.onStressChange.AddListener(StressCurrent);
            PlayerStress.Instance.onFail.AddListener(Fail);
            PlayerActions.Instance.onInteract.AddListener(Interact);
            PlayerActions.Instance.onInteractLong.AddListener(InteractLong);
            //PlayerActions.Instance.onAbility.AddListener(Trigger);
        }

        public float GetInteractAnimationDuration(PlayerActions.InteractAnimation animation)
        {
            if (_interactAnimationDuration.ContainsKey(animation))
            {
                return _interactAnimationDuration[animation].GetAnimationDuration();
            }
            Debug.LogWarning($"The animation type {animation} is not registered in the list");
            return 1;
        }

        public float GetInteractAnimationSpeed(PlayerActions.InteractAnimation animation)
        {
            if (_interactAnimationDuration.ContainsKey(animation))
            {
                return _interactAnimationDuration[animation].Speed;
            }
            Debug.LogWarning($"The animation type {animation} is not registered in the list");
            return 1;
        }

        private void MoveAnimation(Vector2 direction, float speed)
        {
            _animator.SetFloat(MOVE_SPEED, direction.sqrMagnitude);
            _animator.SetFloat(MOVE_SPEED_MULTIPLIER, speed);
            //_animator.SetFloat(MOVE_X, direction.x);
            //_animator.SetFloat(MOVE_Y, direction.y);
        }

        private void Run(bool isRunning)
        {
            _animator.SetBool(RUN, isRunning);
        }

        private void Crouch(bool isCrouching)
        {
            _animator.SetBool(CROUCH, isCrouching);
        }

        private void Interact(PlayerActions.InteractAnimation animation)
        {
            if (animation == PlayerActions.InteractAnimation.None) return;
            _animator.SetFloat(INTERACT_INDEX, (int)animation);
            _animator.SetFloat(INTERACT_SPEED, _interactAnimationDuration[animation].Speed);
            _animator.SetTrigger(INTERACT);
            //if (INTERACT_DICTIONARY.ContainsKey(animation))
            //    _animator.SetTrigger(INTERACT_DICTIONARY[animation]);            
            //else _animator.SetTrigger(INTERACT_DICTIONARY[PlayerActions.InteractAnimation.Default]);
        }

        public void InteractLong(bool isInteracting)
        {
            _animator.SetBool(INTERACT_LONG, isInteracting);
        }

        public void Hold(bool isHolding)
        {
            _animator.SetBool(HOLDING, isHolding);
        }

        public void HeavyHold(bool isHolding)
        {
            _animator.SetBool(HEAVY_HOLDING, isHolding);
        }

        //private void Trigger(string abilityName)
        //{
        //    _animator.SetTrigger(abilityName);
        //}

        public void GoToIdle()
        {
            _animator.SetTrigger(IDLE);
        }

        public void GetUp()
        {
            _animator.SetTrigger(GETUP);
        }

        public void GetUpSit()
        {
            _animator.SetTrigger(GETUP_SIT);
        }

        public void Sit()
        {
            _animator.SetTrigger(SIT);
        }

        private void StressCurrent(float stressCurrent) {
            _animator.SetFloat(STRESS_CURRENT, stressCurrent);
        }

        private void Fail()
        {
            _animator.SetTrigger(FAIL);
        }

        private void Stamina(float currentValue)
        {
            if (currentValue <= _startTiredAnimThreshold)
            {
                _animator.SetLayerWeight(_staminaAnimationLayer, 1 - (currentValue / _startTiredAnimThreshold));
            }
            else _animator.SetLayerWeight(_staminaAnimationLayer, 0);
        }

        public void UseMP3(bool isActive)
        {
            _animator.SetBool(USEMP3, isActive);
        }

        public void TakeDamage(DamageAnimation damageType, bool failState)
        {
            if (damageType == DamageAnimation.None) return;
            _animator.SetFloat(DAMAGE_TYPE, (float)damageType);
            //if (failState)
            //{
            //    _animator.SetLayerWeight(_damageMentalLayer, 0);
            //    _animator.SetLayerWeight(_damagePhysicalLayer, 0);
            //}
            //else
            //{
            //    _animator.SetLayerWeight(_damageMentalLayer, damageType == DamageAnimation.Mental ? 1 : 0);
            //    _animator.SetLayerWeight(_damagePhysicalLayer, damageType == DamageAnimation.Physical ? 1 : 0);
            //    _animator.SetTrigger(TAKE_DAMAGE);
            //}
        }
    }
}