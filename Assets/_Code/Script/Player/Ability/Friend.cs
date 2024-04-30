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
        private NavMeshAgent _navMeshAgent;
        private FriendAnimator _friendAnimator;
        private const float _tick = .2f;
        private const float _distanceExtrapolateFactor = 1.2f;
        private WaitForSeconds _delay = new WaitForSeconds(_tick);
        private float _distanceFromInteractable;
        private float _distanceFromPlayer;
        private bool _isInteracting;
        private Coroutine _rotateCoroutine;

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
            StartCoroutine(BehaviourCoroutine());
        }

        private void OnDisable()
        {
            StopCoroutine(BehaviourCoroutine());
        }

        public void InteractLongWith(IInteractableLong interactableLong)
        {
            InteractableLongCurrent = interactableLong;
            _distanceFromInteractable = Mathf.Round(interactableLong.gameObject.GetComponent<Collider>().bounds.size.magnitude);
            //InteractableLongCurrent.Interact();
        }

        public void InteractLongStop()
        {
            InteractableLongCurrent?.InteractStop();
            _isInteracting = false;
            _navMeshAgent.enabled = true;
            InteractableLongCurrent = null;
        }

        private IEnumerator BehaviourCoroutine()
        {
            while (true)
            {
                //follow player
                if (InteractableLongCurrent == null)
                {
                    _navMeshAgent.stoppingDistance = _distanceFromPlayer;
                    if (_rotateCoroutine != null)
                    {
                        StopCoroutine(_rotateCoroutine);
                        _rotateCoroutine = null;
                    }
                    if (Vector3.Distance(PlayerMovement.Instance.transform.position, transform.position) <= _navMeshAgent.stoppingDistance * _distanceExtrapolateFactor)
                    {
                        //get out of the way of the player
                        _navMeshAgent.SetDestination(PlayerMovement.Instance.transform.position + _navMeshAgent.stoppingDistance * 1.5f * PlayerMovement.Instance.transform.right);
                    }
                    else _navMeshAgent.SetDestination(PlayerMovement.Instance.transform.position);
                    _friendAnimator.UpdateInteracting(false);
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
                _friendAnimator.UpdateWalking(_navMeshAgent.velocity.sqrMagnitude > 0);
                yield return _delay;
            }
        }
        private IEnumerator RotateCoroutine()
        {
            _isInteracting = true;
            _navMeshAgent.enabled = false;
            float count = 0;
            Quaternion initialRotation = transform.rotation;
            Quaternion finalRotation = Quaternion.LookRotation(InteractableLongCurrent.gameObject.transform.position - transform.position).normalized;
            while (count < 1)
            {
                transform.rotation = Quaternion.Lerp(initialRotation, new Quaternion(0, finalRotation.y, 0, finalRotation.w), count);
                count += Time.deltaTime * _rotationSpeed;
                yield return null;
            }
            _friendAnimator.UpdateInteracting(true);
            if (InteractableLongCurrent != null) InteractableLongCurrent.Interact();
            _rotateCoroutine = null;
        }
    }
}
