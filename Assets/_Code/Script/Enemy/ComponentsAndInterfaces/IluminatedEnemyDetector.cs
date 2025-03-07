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
        private WaitForSeconds _paraliseDelay;
        private WaitForSeconds _checkLightDelay;
        private Coroutine _iluminatedCoroutine;
        private Coroutine _checkForLightsCoroutine;
        private bool _isIliuminated;
        private float _baseSpeed;

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
            if (_lightBehaviour == LightBehaviours.Paralise)
            {
                _paraliseDelay = new WaitForSeconds(_paraliseDuration);
                onIlluminated.AddListener((isIlluminated) =>
                {
                    if (isIlluminated) Iluminate();
                    else IluminateStop();
                });
            }
            else
            {
                _checkLightDelay = new WaitForSeconds(_checkLightTickFrequency);
                _checkForLightsCoroutine = StartCoroutine(CheckForLightsCoroutine());
            }
        }

        private void OnDisable()
        {
            if (_iluminatedCoroutine != null) IluminateStop();
            if (_checkForLightsCoroutine != null)
            {
                StopCoroutine(_checkForLightsCoroutine);
                _checkForLightsCoroutine = null;
            }
        }

        [ContextMenu("Iluminate")]
        private void Iluminate()
        {
            if (_lightBehaviour == LightBehaviours.Paralise)
            {
                _isIliuminated = true;
                _baseSpeed = _target.CurrentSpeed;
                _iluminatedCoroutine = StartCoroutine(IluminateCoroutine());
            }
        }

        [ContextMenu("StopIluminate")]
        private void IluminateStop()
        {
            if (_lightBehaviour == LightBehaviours.Paralise)
            {
                StopCoroutine(_iluminatedCoroutine);
                _iluminatedCoroutine = null;
                _isIliuminated = false;
                _target.ChangeSpeed(_baseSpeed);
                _target.UpdateBehaviour(true, true, false);
            }
        }

        private IEnumerator IluminateCoroutine()
        {
            float count;
            while (_isIliuminated)
            {
                count = 0;
                if (_interpolateDuration > 0)
                {
                    while (count < 1)
                    {
                        count += Time.deltaTime / _interpolateDuration;
                        _target.ChangeSpeed(Mathf.Lerp(_baseSpeed, _finalSpeed, _interpolateCurve.Evaluate(count)));
                        yield return null;
                    }
                }
                else
                {
                    _target.ChangeSpeed(_finalSpeed);
                }
                _target.UpdateBehaviour(false, false, true);
                _target.ChangeSpeed(0);
                yield return _paraliseDelay;
                _target.ChangeSpeed(_baseSpeed);
                _target.UpdateBehaviour(true, true, false);
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
                    _target.UpdateBehaviour(false, true, false);
                    _target.ChangeTargetPoint(pos);
                }
                else
                {
                    _target.UpdateBehaviour(true, true, false);
                }
                yield return _checkLightDelay;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_lightBehaviour == LightBehaviours.Paralise && !GetComponent<Collider>()) Debug.LogWarning("The option Paralise from IluminatedEnemy requires a collider, please add one");
        }

        private void OnDrawGizmosSelected()
        {
            if (_lightBehaviour == LightBehaviours.Paralise) return;
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _detectLightRange);
        }
#endif
    }
}
