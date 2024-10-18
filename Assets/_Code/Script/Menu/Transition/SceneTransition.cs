using UnityEngine;
using Ivayami.Scene;
using Ivayami.Player;

namespace Ivayami.UI {
    public class SceneTransition : Fade {

        public static SceneTransition Instance { get; private set; }

        [Header("Scene Transition")]

        [SerializeField] private GameObject _loadingIcon;

        private const string BLOCK_KEY = "ScreenFade";

        protected override void Awake() {
            if (Instance == null) Instance = this;
            else if (Instance != this) {
                Debug.LogWarning($"Multiple instances of {typeof(SceneTransition).Name}, destroying object '{gameObject.name}'");
                Destroy(gameObject);
            }
            base.Awake();

            OnOpenStart.AddListener(() => PlayerMovement.Instance.ToggleMovement(BLOCK_KEY, false));
            OnCloseStart.AddListener(() => PlayerMovement.Instance.ToggleMovement(BLOCK_KEY, true));
            OnTransitionStart.AddListener(() => Pause.Instance.ToggleCanPause(BLOCK_KEY, false));
            OnTransitionEnd.AddListener(() => Pause.Instance.ToggleCanPause(BLOCK_KEY, true));
            OnCloseEnd.AddListener(() => SetLoadingIconState(false));
        }

        private void Start() {
            PlayerMovement.Instance.ToggleMovement(BLOCK_KEY, false);
            Close();
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
