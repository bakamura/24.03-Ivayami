using System.Collections;
using UnityEngine;

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
        private bool _isIliuminated;
        private bool _hasParaliseAnim;
        private bool _paraliseAnimationEnded = true;
        private float _baseSpeed;
        private int _animIndex;
        private bool _reciveDirectLight;

        private void Awake()
        {
            if (!LightFocuses.Instance) return;
            //base.Awake();
            _target = GetComponentInParent<IIluminatedEnemy>();
            if (_target == null)
            {
                Debug.LogWarning("No Illuminated enemy found in hierarchy");
                return;
            }
            if (_lightBehaviour == LightBehaviours.Paralise) onIlluminated.AddListener(UpdateDirectLight);

            _hasParaliseAnim = _enemyAnimator.HasParaliseAnimation();
            _checkLightDelay = new WaitForSeconds(_checkLightTickFrequency);
            _checkForLightsCoroutine = StartCoroutine(CheckForLightsCoroutine());
        }

        private void OnDisable()
        {
            if (_target == null) return;
            IluminateStop();
            if (_checkForLightsCoroutine != null)
            {
                StopCoroutine(_checkForLightsCoroutine);
                _checkForLightsCoroutine = null;
            }
        }

        [ContextMenu("Iluminate")]
        private void Iluminate(bool forceTargetDetect)
        {
            if (!_isIliuminated)
            {
                _isIliuminated = true;
                _baseSpeed = _target.CurrentSpeed;
                _animIndex = Random.Range(0, _paraliseAnimationRandomAmount);
                _iluminatedCoroutine ??= StartCoroutine(IluminateCoroutine(forceTargetDetect));
            }
        }

        [ContextMenu("StopIluminate")]
        private void IluminateStop()
        {
            if (_isIliuminated)
            {
                _isIliuminated = false;
                if (_iluminatedCoroutine != null)
                {
                    StopCoroutine(_iluminatedCoroutine);
                    _iluminatedCoroutine = null;
                }
                if (_hasParaliseAnim)
                {
                    _enemyAnimator.Paralise(false, false, HandleParaliseAnimationEnd, _animIndex);
                }
                else
                {
                    _target.ChangeSpeed(_baseSpeed);
                    _target.UpdateBehaviour(true, true, false, false);
                }
            }
        }
        //the light sent by Lantern
        private void UpdateDirectLight(bool illuminated)
        {
            _reciveDirectLight = illuminated;
        }

        private IEnumerator IluminateCoroutine(bool forceTargetDetect)
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
                    //Debug.Log("IliminateAnimStart");
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
            _iluminatedCoroutine = null;
        }

        private IEnumerator CheckForLightsCoroutine()
        {
            LightFocuses.LightData data;
            bool isIuminated;
            while (true)
            {
                switch (_lightBehaviour)
                {
                    case LightBehaviours.Paralise:
                        isIuminated = false;
                        bool forceTargetDetect = false;
                        if (_reciveDirectLight)
                        {
                            isIuminated = true;
                            forceTargetDetect = true;
                        }
                        else
                        {
                            data = LightFocuses.Instance.GetClosestPointToAreaLight(transform.position);
                            if (data.IsValid() &&
                                !Physics.Raycast(data.Position, (transform.position - data.Position).normalized, Vector3.Distance(data.Position, transform.position), _blockLayers)
                                && Vector3.Distance(transform.position, data.Position) <= _detectLightRange + data.Radius)
                            {
                                isIuminated = true;
                            }
                        }
                        if (isIuminated) Iluminate(forceTargetDetect);
                        else IluminateStop();
                        break;
                    case LightBehaviours.FollowLight:
                        isIuminated = false;
                        data = LightFocuses.Instance.GetClosestPointToAllLights(transform.position);
                        if (data.IsValid() &&
                            !Physics.Raycast(data.Position, (transform.position - data.Position).normalized, Vector3.Distance(data.Position, transform.position), _blockLayers)
                            && Vector3.Distance(transform.position, data.Position) <= _detectLightRange + data.Radius)
                        {
                            isIuminated = true;                            
                        }
                        if (isIuminated)
                        {
                            _target.UpdateBehaviour(false, true, false, false);
                            _target.ChangeTargetPoint(data.Position);
                        }
                        else _target.UpdateBehaviour(true, true, false, false);
                        break;
                }                
                yield return _checkLightDelay;
            }
        }

        private void HandleParaliseAnimationEnd()
        {
            _target.ChangeSpeed(_baseSpeed);
            _target.UpdateBehaviour(true, true, false, false);
            _paraliseAnimationEnded = true;
            //Debug.Log("EndParaliseAnim");
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
            LightFocuses.LightData data = _lightBehaviour == LightBehaviours.Paralise ? 
                LightFocuses.Instance.GetClosestPointToAreaLight(transform.position) : LightFocuses.Instance.GetClosestPointToAllLights(transform.position);
            if (!Physics.Raycast(data.Position, (transform.position - data.Position).normalized, Vector3.Distance(data.Position, transform.position), _blockLayers))
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, data.Position);
        }
#endif
    }
}
