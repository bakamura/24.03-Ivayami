using System.Collections;
using UnityEngine;

namespace Ivayami.Enemy
{
    public class IlluminatedEnemyDetector : Lightable
    {
        [SerializeField] private LightBehaviours _lightBehaviour;
        [SerializeField, Min(0f)] private float _finalSpeed;
        [SerializeField, Min(.02f)] private float _checkLightTickFrequency = .3f;
        [SerializeField, Min(0f)] private float _paraliseDuration;
        [SerializeField, Min(0f)] private float _interpolateDuration;
        [SerializeField] private bool _willInterruptAttack;
        [SerializeField] private LayerMask _blockLayers;
        [SerializeField] private EnemyAnimator _enemyAnimator;
        [SerializeField, Min(0)] private int _paraliseAnimationRandomAmount;
        [SerializeField, Min(0f)] private float _detectLightRange;
#if UNITY_EDITOR
        [SerializeField] private Color _gizmoColor;
#endif
        [SerializeField] private AnimationCurve _interpolateCurve;

        private enum LightBehaviours
        {
            Paralise,
            FollowLight,
            Aggressive
        }
        private IIluminatedEnemy _target;
        private Coroutine _slowMovementCoroutine;
        private Coroutine _checkForLightCoroutine;
        private bool _isIliuminated;
        private bool _hasParaliseAnim;
        private bool _paraliseAnimationEnded = true;
        private float _baseSpeed;
        private int _animIndex;
        private bool _reciveDirectLight;

#if UNITY_EDITOR
        private LightFocuses.LightData _currentLightData;
#endif

        private void Awake()
        {
            if (!LightFocuses.Instance) return;
            //base.Awake();
            _target = GetComponentInParent<IIluminatedEnemy>();
            if (_target == null)
            {
                Debug.LogError("No Illuminated enemy found in hierarchy");
                return;
            }
            _hasParaliseAnim = _enemyAnimator.HasParaliseAnimation();
        }

        private void OnEnable()
        {
            if (!LightFocuses.Instance) return;
            if (_lightBehaviour != LightBehaviours.FollowLight) onIlluminatedByLantern.AddListener(UpdateDirectLight);
            if (_lightBehaviour != LightBehaviours.Aggressive) _checkForLightCoroutine = StartCoroutine(CheckForLightCoroutine());
        }

        private void OnDisable()
        {
            if (!LightFocuses.Instance) return;
            if (_lightBehaviour != LightBehaviours.FollowLight) ParaliseEnd();
            else if (_lightBehaviour == LightBehaviours.FollowLight) _target.UpdateBehaviour(true, true, false, false);
            if (_checkForLightCoroutine != null)
            {
                StopCoroutine(_checkForLightCoroutine);
                _checkForLightCoroutine = null;
            }
        }

        private void ParaliseStart(bool forceTargetDetect)
        {
            if (!_isIliuminated)
            {
                _isIliuminated = true;
                _baseSpeed = _target.CurrentSpeed;
                _animIndex = Random.Range(0, _paraliseAnimationRandomAmount);
                _slowMovementCoroutine ??= StartCoroutine(SlowMovementCoroutine(forceTargetDetect));
            }
        }

        private void ParaliseEnd()
        {
            if (_isIliuminated)
            {
                _isIliuminated = false;
                if (_slowMovementCoroutine != null)
                {
                    StopCoroutine(_slowMovementCoroutine);
                    _slowMovementCoroutine = null;
                }
                if (_hasParaliseAnim)
                {
                    _enemyAnimator.Paralise(false, false, HandleParaliseAnimationEnd, _animIndex);
                }
                else
                {
                    HandleParaliseAnimationEnd();
                }
            }
        }
        //the light sent by Lantern
        private void UpdateDirectLight(bool illuminated)
        {
            _reciveDirectLight = illuminated;
        }

        private IEnumerator CheckForLightCoroutine()
        {
            WaitForSeconds delay = new WaitForSeconds(_checkLightTickFrequency);
            while (true)
            {
                switch (_lightBehaviour)
                {
                    case LightBehaviours.Paralise:
                        HandleChangeAreaLight();
                        break;
                    case LightBehaviours.FollowLight:
                        HandleChangePointLight();
                        break;
                    default:
                        break;
                }
                yield return delay;
            }
        }

        private IEnumerator SlowMovementCoroutine(bool forceTargetDetect)
        {
            float count;
            WaitForFixedUpdate delay = new WaitForFixedUpdate();
            WaitForSeconds paraliseDelay = new WaitForSeconds(_paraliseDuration);
            while (_isIliuminated)
            {
                count = 0;
                if (_interpolateDuration > 0)
                {
                    while (count < 1)
                    {
                        count += Time.fixedDeltaTime / _interpolateDuration;
                        _target.ChangeSpeed(Mathf.Lerp(_baseSpeed, _finalSpeed, _interpolateCurve.Evaluate(count)));
                        yield return delay;
                    }
                }
                else
                {
                    _target.ChangeSpeed(_finalSpeed);
                }
                _target.UpdateBehaviour(false, false, true, forceTargetDetect);
                if (_hasParaliseAnim)
                {
                    _enemyAnimator.Paralise(true, _paraliseAnimationEnded && _willInterruptAttack, paraliseAnimationIndex: _animIndex);
                    //Debug.Log("ParaliseAnimStart");
                    _paraliseAnimationEnded = false;
                }
                if (_paraliseDuration > 0)
                {
                    yield return paraliseDelay;
                    _target.ChangeSpeed(_baseSpeed);
                    _target.UpdateBehaviour(true, true, false, forceTargetDetect);
                }
                else yield break;
            }
            _slowMovementCoroutine = null;
        }

        private void HandleParaliseAnimationEnd()
        {
            _target.ChangeSpeed(_baseSpeed);
            _target.UpdateBehaviour(true, true, false, false);
            _paraliseAnimationEnded = true;
            //Debug.Log("EndParaliseAnim");
        }

        private void HandleChangePointLight()
        {
            LightFocuses.LightData data = LightFocuses.Instance.GetClosestPointToAllLights(transform.position, _detectLightRange);
#if UNITY_EDITOR
            _currentLightData = data;
#endif
            if (data.IsValid() &&
                !Physics.Raycast(data.Position, (transform.position - data.Position).normalized, Vector3.Distance(data.Position, transform.position), _blockLayers))
            {
                _target.UpdateBehaviour(false, true, false, false);
                _target.ChangeTargetPoint(data.Position);
            }
            else _target.UpdateBehaviour(true, true, false, false);
        }

        private void HandleChangeAreaLight()
        {
            bool isIuminated = false;
            bool forceTargetDetect = false;
            if (_reciveDirectLight)
            {
                isIuminated = true;
                forceTargetDetect = true;
#if UNITY_EDITOR
                _currentLightData = new LightFocuses.LightData(Ivayami.Player.PlayerMovement.Instance.transform.position);
#endif
            }
            else
            {
                LightFocuses.LightData data = LightFocuses.Instance.GetClosestPointToAreaLight(transform.position, _detectLightRange);
#if UNITY_EDITOR
                _currentLightData = data;
#endif
                if (data.IsValid() &&
                    !Physics.Raycast(data.Position, (transform.position - data.Position).normalized, Vector3.Distance(data.Position, transform.position), _blockLayers))
                {
                    isIuminated = true;
                }
            }
            if (isIuminated) ParaliseStart(forceTargetDetect);
            else ParaliseEnd();
        }

#if UNITY_EDITOR
        //private void OnValidate()
        //{
        //    if (_lightBehaviour == LightBehaviours.Paralise && !GetComponent<Collider>()) Debug.LogWarning("The option Paralise from IluminatedEnemy requires a collider, please add one");
        //}

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _detectLightRange);
            if (_currentLightData.IsValid())
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;
            if (_currentLightData.IsValid()) Gizmos.DrawLine(transform.position, _currentLightData.Position);
        }
#endif
    }
}

