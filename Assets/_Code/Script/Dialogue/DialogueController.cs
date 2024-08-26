using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System;
using Ivayami.Player;
using Ivayami.Audio;
using Ivayami.UI;
using UnityEngine.UI;
using Ivayami.Save;

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
        [SerializeField] private Image _dialogueBackground;
        [SerializeField] private RectTransform _dialogueContainer;
        [SerializeField] private GameObject _continueDialogueIcon;
        [SerializeField] private DialogueLayout[] _dialogueVariations;
        [SerializeField] private bool _debugLogs;

        private Dictionary<string, List<DialogeRef>> _dialoguesIDs = new Dictionary<string, List<DialogeRef>>();
        private CanvasGroup _canvasGroup;
        //private Dictionary<string, Dialogue> _dialogueDictionary = new Dictionary<string, Dialogue>();
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
        [System.Serializable]
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
            //_autoStartNextDelay = new WaitForSeconds(_delayToAutoStartNextSpeech);
            _canvasGroup = GetComponent<CanvasGroup>();
            _dialogueSounds = GetComponent<DialogueSounds>();

            IsPaused = true;
            Dialogue[] dialogues;
            for (int i = 0; i < Enum.GetNames(typeof(LanguageTypes)).Length; i++)
            {
                dialogues = Resources.LoadAll<Dialogue>($"Dialogues/{Enum.GetName(typeof(LanguageTypes), i)}");
                for (int a = 0; a < dialogues.Length; a++)
                {
                    if (!_dialoguesIDs.ContainsKey(dialogues[a].id))
                    {
                        _dialoguesIDs.Add(dialogues[a].id, new List<DialogeRef>());
                        _dialoguesIDs[dialogues[a].id].Add(new DialogeRef(dialogues[a].GetInstanceID(), (LanguageTypes)i));
                    }
                    else
                    {
                        _dialoguesIDs[dialogues[a].id].Add(new DialogeRef(dialogues[a].GetInstanceID(), (LanguageTypes)i));
                    }
                }
            }
            AsyncOperation operation = Resources.UnloadUnusedAssets();
            operation.completed += (AsyncOperation op) => IsPaused = false;
        }

        //private void Start()
        //{
        //    Options.OnChangeLanguage.AddListener(ChangeLanguage);
        //}

        #region BaseStructure
        public void StartDialogue(string dialogueId, bool lockInput)
        {
            if (IsPaused)
            {
                if (_debugLogs) Debug.Log($"Dialogue is Paused, will not start {dialogueId}");
                return;
            }
            if (TryGetDialogueInstanceID(dialogueId, out int instanceID) /*&& _writtingCoroutine == null*/)
            {
                Dialogue dialogue = (Dialogue)Resources.InstanceIDToObject(instanceID);
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
        }

        private bool TryGetDialogueInstanceID(string dialogueId, out int instanceId)
        {
            if (_dialoguesIDs.ContainsKey(dialogueId))
            {
                for (int i = 0; i < _dialoguesIDs[dialogueId].Count; i++)
                {
                    if (_dialoguesIDs[dialogueId][i].LanguageType == (LanguageTypes)SaveSystem.Instance.Options.language)
                    {
                        instanceId = _dialoguesIDs[dialogueId][i].InstanceID;
                        return true;
                    }
                }
                if (_debugLogs) Debug.LogError($"the dialogue {dialogueId} has not an asset for the language {Enum.GetName(typeof(LanguageTypes), SaveSystem.Instance.Options.language)}");
                instanceId = 0;
                return false;
            }
            else
            {
                if (_debugLogs) Debug.LogError($"the dialogue {dialogueId} is not present in the dictionary");
                instanceId = 0;
                return false;
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
            if (LockInput) _continueDialogueIcon.SetActive(true);
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
                Resources.UnloadAsset(_currentDialogue);
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
            }
            PauseDialogue(false);
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

        public void ChangeLanguage(LanguageTypes languageType)
        {
            //IsPaused = true;

            //for(int i = 0; i < Enum.GetNames(typeof(LanguageTypes)).Length; i++)
            //{
            //    _dialoguesIDs.Add((LanguageTypes)i, Resources.LoadAll<Dialogue>($"Dialogues/{languageType}").Select(x => x.GetInstanceID()).ToList());
            //}
            ////_dialoguesIDs = Resources.LoadAll<Dialogue>($"Dialogues/{languageType}").Select(x => x.GetInstanceID()).ToList();
            ////_dialogueDictionary.Clear();
            ////for (int i = 0; i < _dialogues.Length; i++)
            ////{
            ////    if (!_dialogueDictionary.ContainsKey(_dialogues[i].id))
            ////    {
            ////        _dialogueDictionary.Add(_dialogues[i].id, _dialogues[i]);
            ////    }
            ////    else
            ////    {
            ////        if (_debugLogs)
            ////        {
            ////            Debug.LogWarning($"the dialogue ID {_dialogues[i].id} is already in use");
            ////        }
            ////    }
            ////}
            //AsyncOperation operation = Resources.UnloadUnusedAssets();
            //operation.completed += (AsyncOperation op) => IsPaused = false;
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