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
            _paraliseDelay = new WaitForSeconds(_paraliseDuration);
            _baseSpeed = _enemyPatrol.CurrentSpeed;
        }
        [ContextMenu("Iluminate")]
        public void Iluminated()
        {
            if(_lightBehaviour == LightBehaviours.Paralise)
            {
                _isIliuminated = true;
                StartCoroutine(IluminateCoroutine());
            }
        }

        [ContextMenu("StopIluminate")]
        public void EndLight()
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
    }
}