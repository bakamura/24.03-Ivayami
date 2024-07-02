using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System;
using Ivayami.Player;
using Ivayami.Audio;

//https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/RichText.html

namespace Ivayami.Dialogue
{
    [RequireComponent(typeof(CanvasGroup), typeof(DialogueSounds))]
    public class DialogueController : MonoSingleton<DialogueController>
    {

        [SerializeField, Min(0f)] private float _characterShowDelay;
        //[SerializeField, Min(0f), Tooltip("default value for when dialogue canot be skipped by player input")] private float _delayToAutoStartNextSpeech;
        //[SerializeField] private InputActionAsset _inputActionMap;
        [SerializeField] private InputActionReference _continueInput;
        [SerializeField] private TMP_Text _speechTextComponent;
        [SerializeField] private TMP_Text _announcerNameTextComponent;
        [SerializeField] private GameObject _continueDialogueIcon;
        [SerializeField] private bool _debugLogs;

        private Dialogue[] _dialogues;
        private CanvasGroup _canvasGroup;
        private Dictionary<string, Dialogue> _dialogueDictionary = new Dictionary<string, Dialogue>();
        private Coroutine _writtingCoroutine;
        private WaitForSeconds _typeWrittingDelay;
        //private WaitForSeconds _autoStartNextDelay;
        private Dialogue _currentDialogue;
        private List<DialogueEvents> _dialogueEventsList = new List<DialogueEvents>();
        //private bool _readyForNextSpeech = true;
        private sbyte _currentSpeechIndex;
        private DialogueSounds _dialogueSounds;
        private char[] _currentDialogueCharArray;
        private int _currentCharIndex;
        private float _currentDelay;

        public bool IsPaused { get; private set; }
        public Dialogue CurrentDialogue => _currentDialogue;
        public bool LockInput { get; private set; }
        public Action OnDialogeStart;
        public Action OnDialogueEnd;
        public Action OnSkipSpeech;

        protected override void Awake()
        {
            base.Awake();

            ChangeLanguage(LanguageTypes.ENUS);//TEMPORARY            
            _typeWrittingDelay = new WaitForSeconds(_characterShowDelay);
            //_autoStartNextDelay = new WaitForSeconds(_delayToAutoStartNextSpeech);
            _canvasGroup = GetComponent<CanvasGroup>();
            _dialogueSounds = GetComponent<DialogueSounds>();
        }

        private void HandleContinueDialogue(InputAction.CallbackContext context)
        {
            if (_currentDialogue && _currentDialogue.dialogue[_currentSpeechIndex].FixedDurationInSpeech == 0)
            {
                UpdateDialogue();
            }
        }

        public void UpdateDialogue()
        {
            if (_writtingCoroutine != null)
            {
                SkipSpeech();
            }
            else /*(_writtingCoroutine == null && _readyForNextSpeech)*/
            {
                _currentSpeechIndex++;
                _dialogueSounds.PlaySound(DialogueSounds.SoundTypes.ContinueDialogue);
                //end of current dialogue
                if (_currentSpeechIndex == _currentDialogue.dialogue.Length)
                {
                    StopDialogue();
                }
                //continue current dialogue
                else
                {
                    _continueDialogueIcon.SetActive(false);
                    //_readyForNextSpeech = false;
                    _writtingCoroutine = StartCoroutine(WrittingCoroutine(true));
                }
            }
        }

        private IEnumerator WrittingCoroutine(bool eraseCurrentContent)
        {
            if (eraseCurrentContent)
            {
                _continueDialogueIcon.SetActive(false);
                _announcerNameTextComponent.text = _currentDialogue.dialogue[_currentSpeechIndex].announcerName;
                _speechTextComponent.text = "";
                _currentCharIndex = 0;
                _currentDelay = 0;
                _currentDialogueCharArray = _currentDialogue.dialogue[_currentSpeechIndex].content.ToCharArray();
                ActivateDialogueEvents(_currentDialogue.dialogue[_currentSpeechIndex].eventId);
            }

            _canvasGroup.alpha = _currentDialogueCharArray.Length > 0 ? 1 : 0;
            while (_currentCharIndex < _currentDialogueCharArray.Length)
            {
                if (_currentDialogueCharArray[_currentCharIndex] == '<')
                {
                    int index = _currentCharIndex;
                    while (_currentDialogueCharArray[index] != '>')
                    {
                        _speechTextComponent.text += _currentDialogueCharArray[index];
                        index++;
                    }
                    _currentCharIndex = index;
                }
                else
                {
                    _speechTextComponent.text += _currentDialogueCharArray[_currentCharIndex];
                    _currentCharIndex++;
                    yield return _typeWrittingDelay;
                }
            }

            if (_currentDialogue.dialogue[_currentSpeechIndex].FixedDurationInSpeech > 0 && _currentDelay < _currentDialogue.dialogue[_currentSpeechIndex].FixedDurationInSpeech)
            {
                while (_currentDelay < _currentDialogue.dialogue[_currentSpeechIndex].FixedDurationInSpeech)
                {
                    _currentDelay += Time.deltaTime;
                    yield return null;
                }
            }
            if (LockInput) _continueDialogueIcon.SetActive(true);
            //_readyForNextSpeech = true;
            _writtingCoroutine = null;
            if (_currentDelay > 0)
            {
                if (CutsceneController.IsPlaying) _canvasGroup.alpha = 0;
                else UpdateDialogue();
            }
        }

        private void SkipSpeech()
        {
            if (_debugLogs) Debug.Log($"Skipping typewrite anim");
            StopCoroutine(_writtingCoroutine);
            _speechTextComponent.text = _currentDialogue.dialogue[_currentSpeechIndex].content;
            _announcerNameTextComponent.text = _currentDialogue.dialogue[_currentSpeechIndex].announcerName;
            _continueDialogueIcon.SetActive(true);
            //_readyForNextSpeech = true;
            OnSkipSpeech?.Invoke();
            _writtingCoroutine = null;
        }

        public void StopDialogue()
        {
            if (CurrentDialogue)
            {
                if (_debugLogs) Debug.Log($"End of Dialogue {_currentDialogue.id}");
                if (_writtingCoroutine != null) StopCoroutine(_writtingCoroutine);
                _writtingCoroutine = null;
                ActivateDialogueEvents(_currentDialogue.onEndEventId);
                _currentSpeechIndex = 0;
                _currentDialogue = null;
                _canvasGroup.alpha = 0;
                _canvasGroup.blocksRaycasts = false;
                OnDialogueEnd?.Invoke();
                if (LockInput)
                {
                    PlayerActions.Instance.ChangeInputMap("Player");
                    _continueInput.action.performed -= HandleContinueDialogue;
                    //_continueInput.action.Disable();
                }
                LockInput = false;
                PauseDialogue(false);
            }
        }

        public void UpdateDialogueEventsList(DialogueEvents dialogueEvents)
        {
            if (_dialogueEventsList.Contains(dialogueEvents)) _dialogueEventsList.Remove(dialogueEvents);
            else _dialogueEventsList.Add(dialogueEvents);
        }

        private void ActivateDialogueEvents(string eventID)
        {
            if (!string.IsNullOrEmpty(eventID))
            {
                for (int i = 0; i < _dialogueEventsList.Count; i++)
                {
                    if (_dialogueEventsList[i].TriggerEvent(eventID))
                    {
                        if (_debugLogs) Debug.Log($"Dialogue Event {eventID} Triggered");
                        break;
                    }
                }
            }
        }

        public void StartDialogue(string dialogueId, bool lockInput)
        {
            if (IsPaused)
            {
                if (_debugLogs) Debug.Log($"Dialogue is Paused, will not start {dialogueId}");
                return;
            }
            if (_dialogueDictionary.TryGetValue(dialogueId, out Dialogue dialogue) /*&& _writtingCoroutine == null*/)
            {
                LockInput = lockInput;
                if (LockInput)
                {
                    PlayerActions.Instance.ChangeInputMap("Dialogue");
                    _continueInput.action.performed += HandleContinueDialogue;
                    //_continueInput.action.Enable();
                }
                if (_debugLogs) Debug.Log($"Starting dialogue {dialogueId}");
                _canvasGroup.alpha = 1;
                _canvasGroup.blocksRaycasts = true;
                _currentSpeechIndex = 0;
                _currentDialogue = dialogue;
                OnDialogeStart?.Invoke();
                if (_writtingCoroutine != null)
                {
                    StopCoroutine(_writtingCoroutine);
                    _writtingCoroutine = null;
                }
                _writtingCoroutine = StartCoroutine(WrittingCoroutine(true));
            }
            else
            {
                if (_debugLogs)
                {
                    if (dialogue == null) Debug.LogError($"The dialogue {dialogueId} couldn't be found");
                    //if (_writtingCoroutine != null) Debug.LogWarning($"There is the current dialogue {_currentDialogue.id} playing, the dialogue {dialogueId} will not play");
                }
            }
        }

        public void ChangeLanguage(LanguageTypes languageType)
        {
            _dialogues = Resources.LoadAll<Dialogue>($"Dialogues/{languageType}");
            _dialogueDictionary.Clear();
            for (int i = 0; i < _dialogues.Length; i++)
            {
                if (!_dialogueDictionary.ContainsKey(_dialogues[i].id))
                {
                    _dialogueDictionary.Add(_dialogues[i].id, _dialogues[i]);
                }
                else
                {
                    if (_debugLogs)
                    {
                        Debug.LogWarning($"the dialogue ID {_dialogues[i].id} is already in use");
                    }
                }
            }
        }

        public void PauseDialogue(bool isPaused)
        {
            IsPaused = isPaused;
            if (CurrentDialogue)
            {
                if (isPaused)
                {
                    if (_writtingCoroutine != null) StopCoroutine(_writtingCoroutine);
                    _writtingCoroutine = null;
                }
                else
                {
                    _writtingCoroutine = StartCoroutine(WrittingCoroutine(false));
                }
            }
        }
    }
}