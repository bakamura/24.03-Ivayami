using System.Collections;
using UnityEngine;
using Ivayami.Puzzle;

namespace Ivayami.Enemy
{
    public class IluminatedEnemy : MonoBehaviour, ILightable
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
        private EnemyPatrol _enemyPatrol;
        private WaitForSeconds _paraliseDelay;
        private bool _isIliuminated;
        private float _baseSpeed;

        private void Awake()
        {
            if (!TryGetComponent<EnemyPatrol>(out _enemyPatrol))
                _enemyPatrol = GetComponentInParent<EnemyPatrol>();
            if (_lightBehaviour == LightBehaviours.Paralise)
            {
                _paraliseDelay = new WaitForSeconds(_paraliseDuration);
                _baseSpeed = _enemyPatrol.CurrentSpeed;
            }
            else Ivayami.Player.Ability.Lantern.OnIlluminate.AddListener(HandleIlumatePoint);
        }

        private void OnDisable()
        {
            if(_lightBehaviour == LightBehaviours.FollowLight) Ivayami.Player.Ability.Lantern.OnIlluminate.RemoveListener(HandleIlumatePoint);
        }

        [ContextMenu("Iluminate")]
        public void Iluminate()
        {
            if(_lightBehaviour == LightBehaviours.Paralise)
            {
                _isIliuminated = true;
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
                _enemyPatrol.ChangeSpeed(_baseSpeed);
            }
        }

        private IEnumerator IluminateCoroutine()
        {
            while (_isIliuminated)
            {
                if(_interpolateDuration > 0)
                {
                    float count = 0;
                    while(count < 1)
                    {
                        count += Time.deltaTime / _interpolateDuration;
                        _enemyPatrol.ChangeSpeed(Mathf.Lerp(_baseSpeed, _finalSpeed, _interpolateCurve.Evaluate(count)));
                        yield return null;
                    }
                }
                else
                {
                    _enemyPatrol.ChangeSpeed(_finalSpeed);
                }
                _enemyPatrol.StopBehaviour();
                yield return _paraliseDelay;
                _enemyPatrol.ChangeSpeed(_baseSpeed);
                _enemyPatrol.StartBehaviour();
            }
        }

        private void HandleIlumatePoint(Vector3 point)
        {
            if(point != Vector3.zero)
            {
                _enemyPatrol.UpdateBehaviour(false, true);
                _enemyPatrol.ChangeTargetPoint(point);
            }
            else
            {
                _enemyPatrol.UpdateBehaviour(true, true);
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