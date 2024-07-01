using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using FMODUnity;
using Ivayami.Player;

namespace Ivayami.Dialogue
{
    [RequireComponent(typeof(PlayableDirector))]
    public class CutsceneController : MonoBehaviour
    {
		[Header("UI Components")]
		[SerializeField] private InputActionReference _pauseCutsceneInput;
		[SerializeField] private UnityEvent _onPause;
		[SerializeField] private UnityEvent _onUnpause;

		[Header("Callbacks")]
        [SerializeField] private UnityEvent _onCutsceneStart;
        [SerializeField] private UnityEvent _onCutsceneEnd;

		private PlayableDirector _playableDirector;
		private bool _isPaused;
		public static bool IsPlaying { get; private set; }

        void Start()
        {
            _playableDirector = GetComponent<PlayableDirector>();			

			_onCutsceneStart.AddListener(HandleOnCutsceneStart);
			_onCutsceneEnd.AddListener(HandleOnCutsceneEnd);
            _onCutsceneStart?.Invoke();
            IsPlaying = true;
            _playableDirector.Play();
        }

		private void HandlePauseInput(InputAction.CallbackContext ctx)
        {
			_isPaused = !_isPaused;
            if (_isPaused)
            {
				_playableDirector.Pause();
				RuntimeManager.PauseAllEvents(true);
				DialogueController.Instance.PauseDialogue(true);
				_onPause?.Invoke();
            }
            else
            {
				ResumeCutscene();
			}
		}

		public void SkipCutscene()
		{
			if (!IsPlaying)
				return;

			_playableDirector.Stop();
            if (_isPaused)
            {
				DialogueController.Instance.PauseDialogue(false);
				DialogueController.Instance.StopDialogue();
				RuntimeManager.PauseAllEvents(false);
				_onUnpause?.Invoke();
			}
			IsPlaying = false;
			_onCutsceneEnd?.Invoke();
		}

		public void ResumeCutscene()
        {
			if (!IsPlaying)
				return;

			_playableDirector.Resume();
			DialogueController.Instance.PauseDialogue(false);
			RuntimeManager.PauseAllEvents(false);
			_onUnpause?.Invoke();
		}

		private void HandleOnCutsceneEnd()
        {
			PlayerActions.Instance.ChangeInputMap(null);
			_pauseCutsceneInput.action.performed -= HandlePauseInput;
		}

		private void HandleOnCutsceneStart()
        {
			PlayerActions.Instance.ChangeInputMap("Cutscene");
			_pauseCutsceneInput.action.performed += HandlePauseInput;
		}
	}
}