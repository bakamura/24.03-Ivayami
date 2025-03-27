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
            //if (_lightBehaviour == LightBehaviours.Paralise)
            //{
            //    onIlluminated.AddListener((isIlluminated) =>
            //    {
            //        if (isIlluminated) Iluminate();
            //        else IluminateStop();
            //    });
            //}
            //else
            //{
            //    _checkLightDelay = new WaitForSeconds(_checkLightTickFrequency);
            //    _checkForLightsCoroutine = StartCoroutine(CheckForLightsCoroutine());
            //}
            _checkLightDelay = new WaitForSeconds(_checkLightTickFrequency);
            _checkForLightsCoroutine = StartCoroutine(CheckForLightsCoroutine());
            _hasParaliseAnim = _enemyAnimator.HasParaliseAnimation();
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
        private void Iluminate()
        {            
            if (!_isIliuminated)
            {
                _isIliuminated = true;
                _baseSpeed = _target.CurrentSpeed;
                _animIndex = Random.Range(0, _paraliseAnimationRandomAmount);
                _iluminatedCoroutine ??= StartCoroutine(IluminateCoroutine());
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
                    _target.UpdateBehaviour(true, true, false);
                }
            }
        }

        private IEnumerator IluminateCoroutine()
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
                _target.UpdateBehaviour(false, false, true);
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
                    _target.UpdateBehaviour(true, true, false);
                }
                else yield break;
            }
            _iluminatedCoroutine = null;
        }

        private IEnumerator CheckForLightsCoroutine()
        {
            Vector3 pos;
            while (true)
            {
                pos = LightFocuses.Instance.GetClosestPointTo(transform.position);
                if (pos != Vector3.down && Vector3.Distance(transform.position, pos) <= _detectLightRange)
                {
                    if(_lightBehaviour == LightBehaviours.FollowLight)
                    {
                        _target.UpdateBehaviour(false, true, false);
                        _target.ChangeTargetPoint(pos);
                    }
                    else
                    {
                        Iluminate();
                    }
                }
                else
                {
                    if(_lightBehaviour == LightBehaviours.FollowLight)_target.UpdateBehaviour(true, true, false);
                    else
                    {
                        IluminateStop();
                    }
                }
                yield return _checkLightDelay;
            }
        }

        private void HandleParaliseAnimationEnd()
        {
            _target.ChangeSpeed(_baseSpeed);
            _target.UpdateBehaviour(true, true, false);
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
            EnemyLight[] lights = FindObjectsOfType<EnemyLight>();
            if(lights.Length > 0)
            {
                foreach (EnemyLight light in lights)
                {
                    Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, light.DebugColorAlpha);
                    Gizmos.DrawSphere(light.transform.position, _detectLightRange);
                }
            }
            else
            {
                Gizmos.color = _gizmoColor;
                Gizmos.DrawWireSphere(transform.position, _detectLightRange);
            }
        }
#endif
    }
}
