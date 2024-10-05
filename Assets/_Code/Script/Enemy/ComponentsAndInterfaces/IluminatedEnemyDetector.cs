using System.Collections;
using UnityEngine;
using Ivayami.Puzzle;
using Ivayami.Player.Ability;

namespace Ivayami.Enemy
{
    public class IluminatedEnemyDetector : MonoBehaviour, ILightable
    {
        [SerializeField] private LightBehaviours _lightBehaviour;
        [SerializeField, Min(0f)] private float _finalSpeed;
        [SerializeField, Min(0f)] private float _paraliseDuration;
        [SerializeField, Min(0f)] private float _interpolateDuration;
        [SerializeField] private AnimationCurve _interpolateCurve;

        private enum LightBehaviours
        {
            Paralise,
            FollowLight
        }
        private IIluminatedEnemy _target;
        private WaitForSeconds _paraliseDelay;
        private bool _isIliuminated;
        private float _baseSpeed;

        private void Awake()
        {
            _target = GetComponentInParent<IIluminatedEnemy>();
            if (_target == null)
            {
                Debug.LogWarning("No Illuminated enemy found in hierarchy");
                return;
            }
            if (_lightBehaviour == LightBehaviours.Paralise)
            {
                _paraliseDelay = new WaitForSeconds(_paraliseDuration);
            }
            else Lantern.OnIlluminate.AddListener(HandleIlumatePoint);
        }

        private void OnDisable()
        {
            if (_lightBehaviour == LightBehaviours.FollowLight) Lantern.OnIlluminate.RemoveListener(HandleIlumatePoint);
        }

        [ContextMenu("Iluminate")]
        public void Iluminate()
        {
            if (_lightBehaviour == LightBehaviours.Paralise)
            {
                _isIliuminated = true;
                _baseSpeed = _target.CurrentSpeed;
                StartCoroutine(IluminateCoroutine());
            }
        }

        [ContextMenu("StopIluminate")]
        public void IluminateStop()
        {
            if (_lightBehaviour == LightBehaviours.Paralise)
            {
                StopCoroutine(IluminateCoroutine());
                _isIliuminated = false;
                _target.ChangeSpeed(_baseSpeed);
                _target.UpdateBehaviour(true, true);
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
                _target.UpdateBehaviour(false, false);
                _target.ChangeSpeed(0);
                yield return _paraliseDelay;
                _target.ChangeSpeed(_baseSpeed);
                _target.UpdateBehaviour(true, true);
            }
        }

        private void HandleIlumatePoint(Vector3 point)
        {
            if (point != Vector3.zero)
            {
                _target.UpdateBehaviour(false, true);
                _target.ChangeTargetPoint(point);
            }
            else
            {
                _target.UpdateBehaviour(true, true);
            }
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_lightBehaviour == LightBehaviours.Paralise && !GetComponent<Collider>()) Debug.LogWarning("The option Paralise from IluminatedEnemy requires a collider, please add one");
        }
#endif
    }
}