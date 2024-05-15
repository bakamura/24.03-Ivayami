using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.UI
{
    public class ScreenFade : MonoBehaviour
    {
        private static bool _isFading;
        [SerializeField] private UnityEvent _onFadeEnd;

        public void FadeIn()
        {
            if (!_isFading)
            {
                _isFading = true;
                SceneTransition.Instance.Menu.Open();
                StartCoroutine(WaitFadeCoroutine());
            }
        }
        public void FadeOut()
        {
            if (!_isFading)
            {
                _isFading = true;
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