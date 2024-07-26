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

		[Header("Debug")]
		[SerializeField] private bool _debug;

		private PlayableDirector _playableDirector;
		private bool _isPaused;
		public static bool IsPlaying { get; private set; }

        void Start()
        {
            _playableDirector = GetComponent<PlayableDirector>();			

			_onCutsceneStart.AddListener(HandleOnCutsceneStart);
			_onCutsceneEnd.AddListener(HandleOnCutsceneEnd);
			_playableDirector.stopped += HandleDirectorEnd;
            _onCutsceneStart?.Invoke();
            IsPlaying = true;
            _playableDirector.Play();
			if (_debug) Debug.Log("Cutscene Start");
        }

		private void HandlePauseInput(InputAction.CallbackContext ctx)
        {
			_isPaused = !_isPaused;
            if (_isPaused)
            {
				if (_debug) Debug.Log("Cutscene Pause");
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
				_onUnpause?.Invoke();
			}			
			if (_debug) Debug.Log("Cutscene Skip");
			_onCutsceneEnd?.Invoke();
		}

		public void ResumeCutscene()
        {
			if (!IsPlaying)
				return;

			_isPaused = false;
			_playableDirector.Resume();
			DialogueController.Instance.PauseDialogue(false);
			RuntimeManager.PauseAllEvents(false);
			_onUnpause?.Invoke();
			if (_debug) Debug.Log("Cutscene Resume");
		}

		private void HandleOnCutsceneEnd()
        {
			PlayerActions.Instance.ChangeInputMap(null);
			PlayerActions.Instance.AllowPausing(true);
			DialogueController.Instance.StopDialogue();
			RuntimeManager.PauseAllEvents(false);
			IsPlaying = false;
			_pauseCutsceneInput.action.performed -= HandlePauseInput;
		}

		private void HandleOnCutsceneStart()
        {
			PlayerActions.Instance.ChangeInputMap("Menu");
			PlayerActions.Instance.AllowPausing(false);
			_pauseCutsceneInput.action.performed += HandlePauseInput;
		}

		private void HandleDirectorEnd(PlayableDirector director)
        {
			if (_debug) Debug.Log("Cutscene completed");
			_onCutsceneEnd?.Invoke();
        }
	}
}