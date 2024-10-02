using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using FMODUnity;
using Ivayami.Player;
using Ivayami.UI;
using Ivayami.Audio;

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
            if (!IsPlaying)
                return;

            if (_isPaused)
            {
                _onUnpause?.Invoke();
            }
            _playableDirector.Stop();
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
            PlayerMovement.Instance.UpdateVisualsVisibility(true);
            DialogueController.Instance.StopDialogue();
            DialogueController.Instance.PauseDialogue(false);
            RuntimeManager.PauseAllEvents(false);
            PlayerAudioListener.Instance.UpdateAudioSource(true);
            IsPlaying = false;
            UpdateInputs(false);
        }

        private void HandleOnCutsceneStart()
        {
            PlayerMovement.Instance.UpdateVisualsVisibility(false);
            PlayerAudioListener.Instance.UpdateAudioSource(false);
            UpdateInputs(true);
        }

        private void HandleDirectorEnd(PlayableDirector director)
        {
            if (_debug) Debug.Log("Cutscene completed");
            _onCutsceneEnd?.Invoke();
        }

        private void UpdateInputs(bool isActive)
        {
            PlayerMovement.Instance.ToggleMovement(BLOCK_KEY, !isActive);
            PlayerMovement.Instance.UpdateVisualsVisibility(!isActive);
            Pause.Instance.ToggleCanPause(BLOCK_KEY, !isActive);
            PlayerActions.Instance.ChangeInputMap(isActive ? "Menu" : "Player");
            for (int i = 0; i < _pauseCutsceneInputs.Length; i++)
            {
                if (isActive) _pauseCutsceneInputs[i].action.performed += HandlePauseInput;
                else _pauseCutsceneInputs[i].action.performed -= HandlePauseInput;
            }
        }
    }
}