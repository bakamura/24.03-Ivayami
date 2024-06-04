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
        [SerializeField, Min(0f), Tooltip("default value for when dialogue canot be skipped by player input")] private float _delayToAutoStartNextSpeech;
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

        public bool IsDialogueActive { get; private set; }
        public bool LockInput { get; private set; }
        public Action OnDialogeStart;
        public Action OnDialogueEnd;
        public Action OnSkipSpeech;

        protected override void Awake()
        {
            base.Awake();

            ChangeLanguage(LanguageTypes.ENUS);
            _continueInput.action.performed += HandleContinueDialogue;
            _typeWrittingDelay = new WaitForSeconds(_characterShowDelay);
            //_autoStartNextDelay = new WaitForSeconds(_delayToAutoStartNextSpeech);
            _canvasGroup = GetComponent<CanvasGroup>();
            _dialogueSounds = GetComponent<DialogueSounds>();
        }

        private void HandleContinueDialogue(InputAction.CallbackContext context)
        {
            if (context.ReadValue<float>() == 1 && _currentDialogue && _currentDialogue.dialogue[_currentSpeechIndex].FixedDurationInSpeech == 0)
            {
                UpdateDialogue();
            }
            else
            {
                if (_debugLogs) Debug.Log("Continue Dialogue Input Locked");
            }
        }

        private void UpdateDialogue()
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
                    if (_debugLogs) Debug.Log($"End of Dialogue {_currentDialogue.id}");
                    ActivateDialogueEvents(_currentDialogue.onEndEventId);
                    _currentSpeechIndex = 0;
                    _currentDialogue = null;
                    _canvasGroup.alpha = 0;
                    _canvasGroup.blocksRaycasts = false;
                    OnDialogueEnd?.Invoke();
                    if (LockInput)
                    {
                        PlayerActions.Instance.ChangeInputMap("Player");
                        //_continueInput.action.Disable();
                    }
                    LockInput = false;
                    IsDialogueActive = false;
                }
                //continue current dialogue
                else
                {
                    _continueDialogueIcon.SetActive(false);
                    //_readyForNextSpeech = false;
                    _writtingCoroutine = StartCoroutine(WrittingCoroutine());
                }
            }
        }

        private IEnumerator WrittingCoroutine()
        {
            _continueDialogueIcon.SetActive(false);
            _announcerNameTextComponent.text = _currentDialogue.dialogue[_currentSpeechIndex].announcerName;
            _speechTextComponent.text = "";
            ActivateDialogueEvents(_currentDialogue.dialogue[_currentSpeechIndex].eventId);

            char[] chars = _currentDialogue.dialogue[_currentSpeechIndex].content.ToCharArray();
            _canvasGroup.alpha = chars.Length > 0 ? 1 : 0;
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '<')
                {
                    int index = i;
                    while (chars[index] != '>')
                    {
                        _speechTextComponent.text += chars[index];
                        index++;
                    }
                    _speechTextComponent.text += chars[index];
                    i = index;
                }
                else
                {
                    _speechTextComponent.text += chars[i];
                    yield return _typeWrittingDelay;
                }
            }

            _continueDialogueIcon.SetActive(true);
            WaitForSeconds wait = null;
            if (!LockInput)
                wait = new WaitForSeconds(_delayToAutoStartNextSpeech);
            else if (_currentDialogue.dialogue[_currentSpeechIndex].FixedDurationInSpeech > 0)
                wait = new WaitForSeconds(_currentDialogue.dialogue[_currentSpeechIndex].FixedDurationInSpeech);
            yield return wait;
            //_readyForNextSpeech = true;
            _writtingCoroutine = null;
            if (wait != null) UpdateDialogue();
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
            if (_dialogueDictionary.TryGetValue(dialogueId, out Dialogue dialogue) /*&& _writtingCoroutine == null*/)
            {
                IsDialogueActive = true;
                LockInput = lockInput;
                if (LockInput)
                {
                    PlayerActions.Instance.ChangeInputMap("Dialogue");
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
                _writtingCoroutine = StartCoroutine(WrittingCoroutine());
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

    }
}