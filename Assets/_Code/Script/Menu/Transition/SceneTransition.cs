using UnityEngine;
using Ivayami.Scene;

namespace Ivayami.UI {
    public class SceneTransition : Fade {

        public static SceneTransition Instance { get; private set; }

        [SerializeField] private GameObject _loadingIcon;

        protected override void Awake() {
            if (Instance == null) Instance = this;
            else if (Instance != this) {
                Debug.LogWarning($"Multiple instances of {typeof(SceneTransition).Name}, destroying object '{gameObject.name}'");
                Destroy(gameObject);
            }
            base.Awake();

            OnOpenEnd.AddListener(() => SetLoadingIconState(false)); // Change To Close when fix inversion
        }

        private void Start() {
            Open(); // Change to Close when fix inversion
            SceneController.Instance.OnLoadScene += (sceneName) => SetLoadingIconState(true);
        }

        public void SetDuration(float durationSeconds) {
            _transitionDuration = durationSeconds;
        }

        public void SetAnimationCurve(AnimationCurve animCurve) {
            _transitionCurve = animCurve;
        }

        private void SetLoadingIconState(bool isActive) {
            _loadingIcon.SetActive(isActive);
        }

    }
}
