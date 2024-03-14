using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using TMPro;

namespace Paranapiacaba.Dialogue
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogueController : MonoSingleton<DialogueController>
    {
        [SerializeField] private float _characterShowDelay;
        [SerializeField] private InputActionReference _continueInput;
        [SerializeField] private TMP_Text _speechTextComponent;
        [SerializeField] private TMP_Text _announcerNameTextComponent;
        [SerializeField] private GameObject _continueDialogueIcon;
        [SerializeField] private bool _debugLogs;
        [SerializeField] private Dialogue[] _dialogues;

        private CanvasGroup _canvasGroup;
        private Dictionary<string, Dialogue> _dialogueDictionary = new Dictionary<string, Dialogue>();
        private Coroutine _writtingCoroutine;
        private WaitForSeconds _typeWrittingDelay;
        private Dialogue _currentDialogue;
        private DialogueEvents _currentDialogueEvents;
        private bool _readyForNextSpeech = true;
        private sbyte _currentSpeechIndex;

        protected override void Awake()
        {
            base.Awake();

            _continueInput.action.performed += HandleContinueDialogue;
            _typeWrittingDelay = new WaitForSeconds(_characterShowDelay);
            _canvasGroup = GetComponent<CanvasGroup>();

            for(int i = 0; i < _dialogues.Length; i++)
            {
                if (!_dialogueDictionary.ContainsKey(_dialogues[i].ID))
                {
                    _dialogueDictionary.Add(_dialogues[i].ID, _dialogues[i]);
                }
                else
                {
                    if (_debugLogs)
                    {
                        Debug.LogWarning($"the dialogue ID {_dialogues[i].ID} is already in use");
                    }
                }
            }
        }

        private void HandleContinueDialogue(InputAction.CallbackContext context)
        {
            if (context.ReadValue<float>() == 1)
            {
                UpdateDialogue();
            }
        }

        private void UpdateDialogue()
        {
            if (_writtingCoroutine != null)
            {
                FastForwardDialogue();
            }
            else if (_writtingCoroutine == null && _readyForNextSpeech)
            {
                _currentSpeechIndex++;
                if (_currentSpeechIndex == _currentDialogue.dialogue.Length)
                {
                    //unlock player inputs
                    ActivateDialogueEvents(_currentDialogue.onEndEventId);
                    _readyForNextSpeech = true;
                    _currentDialogue = null;
                    _canvasGroup.alpha = 0;
                    _canvasGroup.blocksRaycasts = false;
                }
                else
                {
                    _readyForNextSpeech = false;
                    _writtingCoroutine = StartCoroutine(WrittingCoroutine());
                }
            }
        }

        [ContextMenu("LoadDialogues")]
        private void LoadDialogues()
        {
            _dialogues = Resources.LoadAll<Dialogue>("Dialogues");
        }

        private IEnumerator WrittingCoroutine()
        {
            _continueDialogueIcon.SetActive(false);
            _announcerNameTextComponent.text = _currentDialogue.dialogue[_currentSpeechIndex].announcerName;
            _speechTextComponent.text = "";
            ActivateDialogueEvents(_currentDialogue.dialogue[_currentSpeechIndex].eventId);

            char[] chars = _currentDialogue.dialogue[_currentSpeechIndex].content.ToCharArray();
            for(int i = 0; i < chars.Length; i++)
            {
                _speechTextComponent.text += chars[i];
                yield return _typeWrittingDelay;
            }
            _readyForNextSpeech = true;
            _writtingCoroutine = null;
        }

        private void FastForwardDialogue()
        {
            _speechTextComponent.text = _currentDialogue.dialogue[_currentSpeechIndex].content;
            _announcerNameTextComponent.text = _currentDialogue.dialogue[_currentSpeechIndex].announcerName;
            _continueDialogueIcon.SetActive(true);
            _readyForNextSpeech = true;
        }

        public void SetCurrentDialogueEvents(DialogueEvents dialogueEvents)
        {
            _currentDialogueEvents = dialogueEvents;
        }

        private void ActivateDialogueEvents(string eventID)
        {
            if (_currentDialogueEvents) _currentDialogueEvents.TriggerEvent(eventID);
        }

        public void StartDialogue(string dialogueId)
        {
            if(_dialogueDictionary.TryGetValue(dialogueId, out Dialogue dialogue) && _writtingCoroutine == null)
            {
                //lock player gamlay inputs and unlock dialogue inputs
                if (_debugLogs) Debug.Log($"Starting dialogue {dialogueId}");
                _canvasGroup.alpha = 1;
                _canvasGroup.blocksRaycasts = true;
                _currentDialogue = dialogue;
                _writtingCoroutine = StartCoroutine(WrittingCoroutine());
            }
            else
            {
                if (_debugLogs)
                {
                    if (dialogue == null) Debug.LogError($"The dialogue {dialogueId} couldn't be found");
                    if (_writtingCoroutine != null) Debug.LogWarning($"There is the current dialogue {_currentDialogue.ID} playing, the dialogue {dialogueId} will not play");
                }
            }
        }
    }
}