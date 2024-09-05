using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using FMODUnity;
using Ivayami.Player;
using Ivayami.UI;

namespace Ivayami.Dialogue
{
    [RequireComponent(typeof(PlayableDirector))]
    public class CutsceneController : MonoBehaviour
    {
		[Header("UI Components")]
		[SerializeField] private InputActionReference[] _pauseCutsceneInputs;
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

		private const string BLOCK_KEY = "Cutscene";

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
			Debug.Log("SkipCutscene()");
			if (!IsPlaying)
				return;

			_playableDirector.Stop();
            if (_isPaused)
            {				
				_onUnpause?.Invoke();
			}			
			if (_debug) Debug.Log("Cutscene Skip");
			//_onCutsceneEnd?.Invoke();
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
			PlayerMovement.Instance.ToggleMovement(BLOCK_KEY, true);
			PlayerActions.Instance.ChangeInputMap("Player");
			PlayerMovement.Instance.UpdateVisualsVisibility(true);
			Pause.Instance.canPause = true;
			DialogueController.Instance.StopDialogue();
			RuntimeManager.PauseAllEvents(false);
			IsPlaying = false;
			UpdateInputs(false);
		}

		private void HandleOnCutsceneStart()
        {
			PlayerMovement.Instance.ToggleMovement(BLOCK_KEY, false);
			PlayerMovement.Instance.UpdateVisualsVisibility(false);
            Pause.Instance.canPause = false;
            PlayerActions.Instance.ChangeInputMap("Menu");
			UpdateInputs(true);
		}

		private void HandleDirectorEnd(PlayableDirector director)
        {
			if (_debug) Debug.Log("Cutscene completed");
			_onCutsceneEnd?.Invoke();
        }

		private void UpdateInputs(bool isActive)
        {
			for(int i = 0; i < _pauseCutsceneInputs.Length; i++)
            {
				if (isActive) _pauseCutsceneInputs[i].action.performed += HandlePauseInput;
				else _pauseCutsceneInputs[i].action.performed -= HandlePauseInput;
			}
        }
	}
}