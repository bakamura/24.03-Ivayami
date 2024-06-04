using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.UI
{
    public class ScreenFade : MonoBehaviour
    {
        private static bool _isFading;
        [SerializeField, Min(0f)] private float _duration = 1f;
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.Linear(0,0,1,1);
        [SerializeField] private UnityEvent _onFadeEnd;

        public void FadeIn()
        {
            if (!_isFading)
            {
                _isFading = true;
                if(_duration > 0f) SceneTransition.Instance.SetDuration(_duration);
                SceneTransition.Instance.SetAnimationCurve(_fadeCurve);
                SceneTransition.Instance.Menu.Open();
                StartCoroutine(WaitFadeCoroutine());
            }
        }
        public void FadeOut()
        {
            if (!_isFading)
            {
                _isFading = true;
                if (_duration > 0f) SceneTransition.Instance.SetDuration(_duration);
                SceneTransition.Instance.SetAnimationCurve(_fadeCurve);
                SceneTransition.Instance.Menu.Close();
                StartCoroutine(WaitFadeCoroutine());
            }
        }

        private IEnumerator WaitFadeCoroutine()
        {
            float count = 0;
            while(count < SceneTransition.Instance.Menu.TransitionDuration)
            {
                count += Time.deltaTime;
                yield return null;
            }
            _isFading = false;
            _onFadeEnd?.Invoke();
        }
    }
}