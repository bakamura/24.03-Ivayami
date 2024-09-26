using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System;
using Ivayami.Player;
using Ivayami.Audio;
using UnityEngine.UI;
using Ivayami.Save;

//https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/RichText.html

namespace Ivayami.Dialogue
{
    [RequireComponent(typeof(CanvasGroup), typeof(DialogueSounds))]
    public class DialogueController : MonoSingleton<DialogueController>
    {
        [SerializeField, Min(0f)] private float _characterShowDelay;
        [SerializeField] private InputActionReference _continueInput;
        [SerializeField] private TMP_Text _speechTextComponent;
        [SerializeField] private TMP_Text _textToCopyFrom;
        [SerializeField] private TMP_Text _announcerNameTextComponent;
        [SerializeField] private Image _dialogueBackground;
        [SerializeField] private RectTransform _dialogueContainer;
        [SerializeField] private GameObject _continueDialogueIcon;
        [SerializeField] private DialogueLayout[] _dialogueVariations;
        [SerializeField] private bool _debugLogs;

        private Dictionary<string, int> _dialoguesIDs = new Dictionary<string, int>();
        private CanvasGroup _canvasGroup;
        private Coroutine _writtingCoroutine;
        private WaitForSeconds _typeWrittingDelay;
        private Dialogue _currentDialogue;
        private List<DialogueEvents> _dialogueEventsList = new List<DialogueEvents>();
        private DialogueSounds _dialogueSounds;
        private RectTransform _textSpeechRect;
        private char[] _currentDialogueCharArray;
        private int _currentCharIndex;
        private sbyte _currentSpeechIndex;
        [Serializable]
        private struct DialogueLayout
        {
            public Sprite Background;
            public Vector2 Dimensions;
        }

        private struct DialogeRef
        {
            public int InstanceID;
            public LanguageTypes LanguageType;

            public DialogeRef(int id, LanguageTypes type)
            {
                InstanceID = id;
                LanguageType = type;
            }
        }

        public bool IsPaused { get; private set; }
        public Dialogue CurrentDialogue => _currentDialogue;
        public bool LockInput { get; private set; }
        public Action OnDialogeStart;
        public Action OnDialogueEnd;
        public Action OnSkipSpeech;

        protected override void Awake()
        {
            base.Awake();

            _typeWrittingDelay = new WaitForSeconds(_characterShowDelay);
            _canvasGroup = GetComponent<CanvasGroup>();
            _dialogueSounds = GetComponent<DialogueSounds>();
            _textSpeechRect = _speechTextComponent.GetComponent<RectTransform>();

            IsPaused = true;
            Dialogue[] dialogues;
            dialogues = Resources.LoadAll<Dialogue>("Dialogues");
            string assetName;
            for (int i = 0; i < dialogues.Length; i++)
            {
                assetName = dialogues[i].name;
                if (!_dialoguesIDs.ContainsKey(assetName))
                {
                    _dialoguesIDs.Add(assetName, dialogues[i].ID);
                }
                else
                {
                    Debug.LogWarning($"Dialogue {dialogues[i].name} Duplicate Found, please delete it");
                }
                //Resources.UnloadAsset(dialogues[i]);
            }
            AsyncOperation operation = Resources.UnloadUnusedAssets();
            operation.completed += (AsyncOperation op) => { IsPaused = false; };
        }

        #region BaseStructure
        public void StartDialogue(string dialogueName, bool lockInput)
        {
            if (IsPaused)
            {
                if (_debugLogs) Debug.Log($"Dialogue is Paused, will not start {dialogueName}");
                return;
            }
            if (TryGetDialogueInstanceID(dialogueName, out int instanceID))
            {
                if (_currentDialogue) StopDialogue();
                Dialogue dialogue = (Dialogue)Resources.InstanceIDToObject(instanceID);
                LockInput = lockInput;
                if (LockInput)
                {
                    PlayerActions.Instance.ChangeInputMap("Dialogue");
                    _continueInput.action.performed += HandleContinueDialogue;
                }
                if (_debugLogs) Debug.Log($"Starting dialogue {dialogueName}");
                _canvasGroup.alpha = 1;
                _canvasGroup.blocksRaycasts = true;
                _currentSpeechIndex = 0;
                _currentDialogue = dialogue;
                OnDialogeStart?.Invoke();
                _writtingCoroutine = StartCoroutine(WrittingCoroutine(true));
            }
        }

        private bool TryGetDialogueInstanceID(string dialogueName, out int instanceId)
        {
            instanceId = 0;
            if (_dialoguesIDs.ContainsKey(dialogueName))
            {
                instanceId = _dialoguesIDs[dialogueName];
                return true;
            }
            else
            {
                Debug.LogError($"the dialogue {dialogueName} is not present in the dictionary");
                return false;
            }
        }
        public void UpdateDialogue()
        {
            if (_writtingCoroutine != null)
            {
                SkipSpeech();
            }
            else
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
                    _writtingCoroutine = StartCoroutine(WrittingCoroutine(true));
                }
            }
        }

        private IEnumerator WrittingCoroutine(bool eraseCurrentContent)
        {
            if (eraseCurrentContent)
            {
                _continueDialogueIcon.SetActive(false);
                _announcerNameTextComponent.text = _currentDialogue.dialogue[_currentSpeechIndex].Speeches[SaveSystem.Instance.Options.language].announcerName;
                _speechTextComponent.text = null;
                _textToCopyFrom.text = _currentDialogue.dialogue[_currentSpeechIndex].Speeches[SaveSystem.Instance.Options.language].content;
                _currentCharIndex = 0;
                _currentDialogueCharArray = _currentDialogue.dialogue[_currentSpeechIndex].Speeches[SaveSystem.Instance.Options.language].content.ToCharArray();
                if (CutsceneController.IsPlaying || !LockInput)
                {
                    _dialogueBackground.sprite = _dialogueVariations[1].Background;
                    _dialogueContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _dialogueVariations[1].Dimensions.x);
                    _dialogueContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _dialogueVariations[1].Dimensions.y);
                }
                else
                {
                    _dialogueBackground.sprite = _dialogueVariations[0].Background;
                    _dialogueContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _dialogueVariations[0].Dimensions.x);
                    _dialogueContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _dialogueVariations[0].Dimensions.y);
                }
                ActivateDialogueEvents(_currentDialogue.dialogue[_currentSpeechIndex].EventId);
            }
            _textSpeechRect.rect.Set(_textSpeechRect.rect.x, _textSpeechRect.rect.y, _textToCopyFrom.preferredWidth, _textToCopyFrom.preferredHeight);
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

            yield return new WaitForSeconds(_currentDialogue.dialogue[_currentSpeechIndex].FixedDurationInSpeech);
            if (LockInput) _continueDialogueIcon.SetActive(true);
            _writtingCoroutine = null;
            if (_currentDialogue.dialogue[_currentSpeechIndex].FixedDurationInSpeech > 0)
            {
                if (CutsceneController.IsPlaying) _canvasGroup.alpha = 0;
                else UpdateDialogue();
            }
        }

        private void SkipSpeech()
        {
            if (_debugLogs) Debug.Log($"Skipping typewrite anim");
            StopCoroutine(_writtingCoroutine);
            _speechTextComponent.text = _currentDialogue.dialogue[_currentSpeechIndex].Speeches[SaveSystem.Instance.Options.language].content;
            _announcerNameTextComponent.text = _currentDialogue.dialogue[_currentSpeechIndex].Speeches[SaveSystem.Instance.Options.language].announcerName;
            if (LockInput) _continueDialogueIcon.SetActive(true);
            OnSkipSpeech?.Invoke();
            _writtingCoroutine = null;
        }

        public void StopDialogue()
        {
            if (CurrentDialogue)
            {
                if (_debugLogs) Debug.Log($"End of Dialogue {_currentDialogue.name}");
                if (_writtingCoroutine != null) StopCoroutine(_writtingCoroutine);
                _writtingCoroutine = null;
                ActivateDialogueEvents(_currentDialogue.onEndEventId);
                _currentSpeechIndex = 0;
                Resources.UnloadAsset(_currentDialogue);
                _currentDialogue = null;
                _canvasGroup.alpha = 0;
                _canvasGroup.blocksRaycasts = false;
                OnDialogueEnd?.Invoke();
                if (LockInput)
                {
                    PlayerActions.Instance.ChangeInputMap("Player");
                    _continueInput.action.performed -= HandleContinueDialogue;
                }
                LockInput = false;
                IsPaused = false;
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
        #endregion

        #region InputCallbacks
        private void HandleContinueDialogue(InputAction.CallbackContext context)
        {
            if (_currentDialogue && _currentDialogue.dialogue[_currentSpeechIndex].FixedDurationInSpeech == 0)
            {
                UpdateDialogue();
            }
        }
        #endregion        
    }
}