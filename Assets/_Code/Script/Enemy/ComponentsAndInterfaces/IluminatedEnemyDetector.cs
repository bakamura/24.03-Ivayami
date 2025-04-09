using System.Collections;
using UnityEngine;
using Ivayami.Puzzle;

namespace Ivayami.Enemy
{
    public class IluminatedEnemyDetector : Lightable
    {
        [SerializeField] private LightBehaviours _lightBehaviour;
        [SerializeField, Min(0f)] private float _finalSpeed;
        [SerializeField, Min(0f)] private float _paraliseDuration;
        [SerializeField, Min(0f)] private float _interpolateDuration;
        [SerializeField] private bool _willInterruptAttack;
        [SerializeField] private LayerMask _blockLayers;
        [SerializeField] private EnemyAnimator _enemyAnimator;
        [SerializeField, Min(0)] private int _paraliseAnimationRandomAmount;
        [SerializeField, Min(0f)] private float _detectLightRange;
        [SerializeField, Min(0.02f)] private float _checkLightTickFrequency = 1;
        [SerializeField] private Color _gizmoColor;
        [SerializeField] private AnimationCurve _interpolateCurve;

        private enum LightBehaviours
        {
            Paralise,
            FollowLight
        }
        private IIluminatedEnemy _target;
        private WaitForSeconds _checkLightDelay;
        private Coroutine _iluminatedCoroutine;
        private Coroutine _checkForLightsCoroutine;
        private bool _isIlluminated;
        private bool _hasParaliseAnim;
        private bool _paraliseAnimationEnded = true;
        private float _baseSpeed;
        private int _animIndex;

        private bool _wasLitByFocuses;
        private bool _isLitByFocuses;
        private const string FOCUSES_KEY = "LightFocuses Illumination";

        protected override void Awake()
        {
            if (!LightFocuses.Instance) return;
            base.Awake();
            
            _target = GetComponentInParent<IIluminatedEnemy>();
            if (_target == null)
            {
                Debug.LogWarning("No Illuminated enemy found in hierarchy");
                return;
            }
            _hasParaliseAnim = _enemyAnimator.HasParaliseAnimation();
            _checkLightDelay = new WaitForSeconds(_checkLightTickFrequency);
            _checkForLightsCoroutine = StartCoroutine(IlluminatedByFocusesCheck());
            
        }

        private void OnEnable() {
            onIlluminated.AddListener(IlluminateHandler);
            LightFocuses.OnChange.AddListener(FocusesChangeHandle);
        }

        private void OnDisable()
        {
            if (_target == null) return;
            IlluminateHandler(false);
            if (_checkForLightsCoroutine != null)
            {
                StopCoroutine(_checkForLightsCoroutine);
                _checkForLightsCoroutine = null;
            }
            onIlluminated.RemoveListener(IlluminateHandler);
            LightFocuses.OnChange.RemoveListener(FocusesChangeHandle);
        }

        private void IlluminateHandler(bool isIlluminated)
        {            
            if (_isIlluminated != isIlluminated)
            {
                _isIlluminated = isIlluminated;
                if(_isIlluminated) {
                    _iluminatedCoroutine ??= StartCoroutine(IluminateCoroutine());
                }
                else {
                    if (_iluminatedCoroutine != null) {
                        StopCoroutine(_iluminatedCoroutine);
                        _iluminatedCoroutine = null;
                    }
                    if (_hasParaliseAnim) {
                        _enemyAnimator.Paralise(false, false, HandleParaliseAnimationEnd, _animIndex);
                    }
                    else {
                        _target.ChangeSpeed(_baseSpeed);
                        _target.UpdateBehaviour(true, true, false);
                    }
                }
            }
        }

        private void FocusesChangeHandle() {
            if (_lightBehaviour == LightBehaviours.FollowLight) {
                _target.UpdateBehaviour(!_isLitByFocuses, true, false);
                if (_isLitByFocuses) _target.ChangeTargetPoint(LightFocuses.Instance.GetClosestPointTo(transform.position).Position);
            }
        }

        private IEnumerator IlluminatedByFocusesCheck()
        {
            while (true)
            {
                _wasLitByFocuses = _isLitByFocuses;
                _isLitByFocuses = LightFocuses.Instance.IsPointInsideLightRange(transform.position, _detectLightRange);
                if (_wasLitByFocuses != _isLitByFocuses) Illuminate(FOCUSES_KEY, _isLitByFocuses);
                
                yield return _checkLightDelay;
            }
        }

        private IEnumerator IluminateCoroutine()
        {
            WaitForFixedUpdate delay = new WaitForFixedUpdate();
            WaitForSeconds paraliseDelay = new WaitForSeconds(_paraliseDuration);
            _baseSpeed = _target.CurrentSpeed;
            _animIndex = Random.Range(0, _paraliseAnimationRandomAmount);
            while (_isIlluminated)
            {
                if (_interpolateDuration > 0)
                {
                    float count = 0;
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

                _target.UpdateBehaviour(false, false, true);
                if (_hasParaliseAnim)
                {
                    _enemyAnimator.Paralise(true, _paraliseAnimationEnded && _willInterruptAttack, paraliseAnimationIndex: _animIndex);
                    _paraliseAnimationEnded = false;
                }
                if (_paraliseDuration > 0)
                {
                    yield return paraliseDelay;
                    _target.ChangeSpeed(_baseSpeed);
                    _target.UpdateBehaviour(true, true, false);
                }
                else yield break;
            }
            _iluminatedCoroutine = null;
        }

        private void HandleParaliseAnimationEnd()
        {
            _target.ChangeSpeed(_baseSpeed);
            _target.UpdateBehaviour(true, true, false);
            _paraliseAnimationEnded = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_lightBehaviour == LightBehaviours.Paralise && !GetComponent<Collider>()) Debug.LogWarning("The option Paralise from IluminatedEnemy requires a collider, please add one");
        }

        private void OnDrawGizmosSelected()
        {
            //if (_lightBehaviour == LightBehaviours.Paralise) return;
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _detectLightRange);
            LightFocuses.LightData data = LightFocuses.Instance.GetClosestPointTo(transform.position);
            if (!Physics.Raycast(data.Position, (transform.position - data.Position).normalized, Vector3.Distance(data.Position, transform.position), _blockLayers))
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, data.Position);
        }
#endif

    }
}
