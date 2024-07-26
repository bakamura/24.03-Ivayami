using Ivayami.Puzzle;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace Ivayami.Player.Ability
{
    [RequireComponent(typeof(NavMeshAgent), typeof(FriendAnimator))]
    public class Friend : MonoSingleton<Friend>
    {
        [SerializeField, Min(.01f)] private float _rotationSpeed = 1;
        [SerializeField, Tooltip("Force a specific direction to look when in match animation type relative to the object")] private AnimationOrientation[] _animationsOrientation;
        private NavMeshAgent _navMeshAgent;
        private FriendAnimator _friendAnimator;
        private const float _tick = .2f;
        private const float _distanceExtrapolateFactor = 1.2f;
        private WaitForSeconds _delay = new WaitForSeconds(_tick);
        private float _distanceFromInteractable;
        private float _distanceFromPlayer;
        private bool _isInteracting;
        private Coroutine _rotateCoroutine;
        private Coroutine _behaviourCoroutine;
        private PlayerActions.InteractAnimation _currentInteractionType;
        [System.Serializable]
        private struct AnimationOrientation
        {
            public PlayerActions.InteractAnimation AnimatyonType;
            public DirectionTypes LookDirection;
        }
        private enum DirectionTypes
        {
            Forward,
            Back,
            Left,
            Right
        }

        public IInteractableLong InteractableLongCurrent { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _friendAnimator = GetComponent<FriendAnimator>();
            _distanceFromPlayer = _navMeshAgent.stoppingDistance;
        }

        private void OnEnable()
        {
            ActivateBehaviour();
        }

        private void OnDisable()
        {
            DeactivateBehaviour();
        }

        [ContextMenu("Activate")]
        public void ActivateBehaviour()
        {
            if (_behaviourCoroutine == null) _behaviourCoroutine = StartCoroutine(BehaviourCoroutine());
        }

        [ContextMenu("Deactivate")]
        public void DeactivateBehaviour()
        {
            if (_behaviourCoroutine != null)
            {
                StopCoroutine(_behaviourCoroutine);
                _behaviourCoroutine = null;
            }
        }

        public void InteractLongWith(IInteractableLong interactableLong)
        {
            InteractableLongCurrent = interactableLong;
            _distanceFromInteractable = Mathf.Round(interactableLong.gameObject.GetComponent<Collider>().bounds.size.magnitude);
        }

        public void InteractLongStop()
        {
            InteractableLongCurrent?.InteractStop();
            if (_rotateCoroutine != null)
            {
                StopCoroutine(_rotateCoroutine);
                _rotateCoroutine = null;
            }
            _isInteracting = false;
            _navMeshAgent.enabled = true;
            _friendAnimator.UpdateInteraction(_currentInteractionType, false);
            InteractableLongCurrent = null;
        }

        public void GoToPosition(Transform transform)
        {
            DeactivateBehaviour();
            _navMeshAgent.SetDestination(transform.position);
        }

        public void ChangeSpeed(float speed)
        {
            _navMeshAgent.speed = speed;
            _friendAnimator.UpdateWalking(_navMeshAgent.velocity.sqrMagnitude / (_navMeshAgent.speed * _navMeshAgent.speed));
        }

        private IEnumerator BehaviourCoroutine()
        {
            while (true)
            {
                if (_navMeshAgent.enabled)
                {
                    //follow player
                    if (InteractableLongCurrent == null)
                    {
                        _navMeshAgent.stoppingDistance = _distanceFromPlayer;
                        if (Vector3.Distance(PlayerMovement.Instance.transform.position, transform.position) <= _navMeshAgent.stoppingDistance * _distanceExtrapolateFactor)
                        {
                            //get out of the way of the player
                            _navMeshAgent.SetDestination(PlayerMovement.Instance.transform.position + _navMeshAgent.stoppingDistance * 1.5f * PlayerMovement.Instance.transform.right);
                        }
                        else _navMeshAgent.SetDestination(PlayerMovement.Instance.transform.position);
                        //_friendAnimator.UpdateInteracting(false);
                    }
                    //interact
                    else
                    {
                        _navMeshAgent.stoppingDistance = _distanceFromInteractable;
                        if (Vector3.Distance(InteractableLongCurrent.gameObject.transform.position, transform.position) <= _navMeshAgent.stoppingDistance * _distanceExtrapolateFactor)
                        {
                            if (!_isInteracting) _rotateCoroutine = StartCoroutine(RotateCoroutine());
                        }
                        else
                        {
                            _navMeshAgent.SetDestination(InteractableLongCurrent.gameObject.transform.position);
                        }
                    }
                    _friendAnimator.UpdateWalking(_navMeshAgent.velocity.sqrMagnitude / (_navMeshAgent.speed * _navMeshAgent.speed));
                }
                yield return _delay;
            }
        }
        private IEnumerator RotateCoroutine()
        {
            _isInteracting = true;
            _navMeshAgent.enabled = false;
            float count = 0;
            Quaternion initialRotation = transform.rotation;
            Quaternion finalRotation;
            if (TryGetSpecifiedLookDirection(_currentInteractionType, out Vector3 direction))
            {
                finalRotation = Quaternion.LookRotation(direction);
            }
            else finalRotation = Quaternion.LookRotation(InteractableLongCurrent.gameObject.transform.position - transform.position).normalized;
            while (count < 1)
            {
                transform.rotation = Quaternion.Lerp(initialRotation, new Quaternion(0, finalRotation.y, 0, finalRotation.w), count);
                count += Time.deltaTime * _rotationSpeed;
                yield return null;
            }
            if (InteractableLongCurrent != null)
            {
                _currentInteractionType = InteractableLongCurrent.Interact();
                _friendAnimator.UpdateInteraction(_currentInteractionType, true);
            }
            _rotateCoroutine = null;
        }

        private bool TryGetSpecifiedLookDirection(PlayerActions.InteractAnimation animation, out Vector3 direction)
        {
            direction = Vector3.zero;
            for (int i = 0; i < _animationsOrientation.Length; i++)
            {
                if (_animationsOrientation[i].AnimatyonType == animation)
                {
                    switch (_animationsOrientation[i].LookDirection)
                    {
                        case DirectionTypes.Forward:
                            direction = InteractableLongCurrent.gameObject.transform.forward;
                            break;
                        case DirectionTypes.Back:
                            direction = -InteractableLongCurrent.gameObject.transform.forward;
                            break;
                        case DirectionTypes.Left:
                            direction = -InteractableLongCurrent.gameObject.transform.right;
                            break;
                        case DirectionTypes.Right:
                            direction = InteractableLongCurrent.gameObject.transform.right;
                            break;
                    }
                    direction += new Vector3(0, .25f, 0);
                    return true;
                }
            }
            return false;
        }
    }
}
