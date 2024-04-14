using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System;
using Paranapiacaba.Player;

//https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/RichText.html

namespace Paranapiacaba.Dialogue {
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogueController : MonoSingleton<DialogueController> {

        [SerializeField] private float _characterShowDelay;
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
        private Dialogue _currentDialogue;
        private DialogueEvents _currentDialogueEvents;
        private bool _readyForNextSpeech = true;
        private sbyte _currentSpeechIndex;

        public bool IsDialogueActive { get; private set; }
        public Action OnDialogeStart;
        public Action OnDialogueEnd;
        public Action OnSkipSpeech;

        protected override void Awake()
        {
            base.Awake();

            _dialogues = Resources.LoadAll<Dialogue>("Dialogues");            
            _continueInput.action.performed += HandleContinueDialogue;
            _typeWrittingDelay = new WaitForSeconds(_characterShowDelay);
            _canvasGroup = GetComponent<CanvasGroup>();

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

        private void HandleContinueDialogue(InputAction.CallbackContext context)
        {
            if (context.ReadValue<float>() == 1 && _currentDialogue)
            {
                UpdateDialogue();
            }
        }

        private void UpdateDialogue()
        {
            if (_writtingCoroutine != null)
            {
                SkipSpeech();
            }
            else if (_writtingCoroutine == null && _readyForNextSpeech)
            {
                _currentSpeechIndex++;
                //end of current dialogue
                if (_currentSpeechIndex == _currentDialogue.dialogue.Length)
                {
                    _continueInput.action.Disable();
                    ActivateDialogueEvents(_currentDialogue.onEndEventId);
                    _currentSpeechIndex = 0;
                    _currentDialogue = null;
                    _canvasGroup.alpha = 0;
                    _canvasGroup.blocksRaycasts = false;
                    OnDialogueEnd?.Invoke();
                    PlayerActions.Instance.ChangeInputMap("Player");
                    //_inputActionMap.actionMaps[0].Enable();
                    IsDialogueActive = false;
                }
                //continue current dialogue
                else
                {
                    _continueDialogueIcon.SetActive(false);
                    _readyForNextSpeech = false;
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
            _readyForNextSpeech = true;
            _writtingCoroutine = null;
        }

        private void SkipSpeech()
        {
            StopCoroutine(_writtingCoroutine);
            _speechTextComponent.text = _currentDialogue.dialogue[_currentSpeechIndex].content;
            _announcerNameTextComponent.text = _currentDialogue.dialogue[_currentSpeechIndex].announcerName;
            _continueDialogueIcon.SetActive(true);
            _readyForNextSpeech = true;
            OnSkipSpeech?.Invoke();
            _writtingCoroutine = null;
        }

        public void SetCurrentDialogueEvents(DialogueEvents dialogueEvents)
        {
            _currentDialogueEvents = dialogueEvents;
        }

        private void ActivateDialogueEvents(string eventID)
        {
            if (_currentDialogueEvents && !string.IsNullOrEmpty(eventID)) _currentDialogueEvents.TriggerEvent(eventID);
        }

        public void StartDialogue(string dialogueId)
        {
            if (_dialogueDictionary.TryGetValue(dialogueId, out Dialogue dialogue) && _writtingCoroutine == null)
            {
                IsDialogueActive = true;
                PlayerActions.Instance.ChangeInputMap("Dialogue");
                //_inputActionMap.actionMaps[0].Disable();
                _continueInput.action.Enable();
                if (_debugLogs) Debug.Log($"Starting dialogue {dialogueId}");
                _canvasGroup.alpha = 1;
                _canvasGroup.blocksRaycasts = true;
                _currentDialogue = dialogue;
                OnDialogeStart?.Invoke();
                _writtingCoroutine = StartCoroutine(WrittingCoroutine());
            }
            else
            {
                if (_debugLogs)
                {
                    if (dialogue == null) Debug.LogError($"The dialogue {dialogueId} couldn't be found");
                    if (_writtingCoroutine != null) Debug.LogWarning($"There is the current dialogue {_currentDialogue.id} playing, the dialogue {dialogueId} will not play");
                }
            }
        }

    }
}