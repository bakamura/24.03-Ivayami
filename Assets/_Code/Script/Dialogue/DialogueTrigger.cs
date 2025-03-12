using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private Dialogue _dialogue;
        [SerializeField] private UnityEvent _onDialogueStart;
        [SerializeField] private UnityEvent _onDialogueEnd;
        [SerializeField, ReadOnly] private string _dialogueName;
        [SerializeField] private bool _activateOnce;
        [SerializeField] private bool _deactivateObjectOnFirstActivate;
        [SerializeField] private bool _lockPlayerInput;

        private bool _activated;

        private void Start()
        {
            if (_dialogue)
            {
                _dialogueName = _dialogue.name;
                Resources.UnloadAsset(_dialogue);
            }
        }

        [ContextMenu("StartDialogue")]
        public void StartDialogue()
        {
            if (!_activateOnce || (_activateOnce && !_activated))
            {
                if (_onDialogueStart.GetPersistentEventCount() > 0)
                {
                    DialogueController.Instance.OnDialogueStart += _onDialogueStart.Invoke;
                    DialogueController.Instance.OnDialogueEnd += UnsubscribeOnDialogueStart;
                }
                if (_onDialogueEnd.GetPersistentEventCount() > 0)
                {
                    DialogueController.Instance.OnDialogueEnd += _onDialogueEnd.Invoke;
                    DialogueController.Instance.OnDialogueEnd += UnsubscribeOnDialogueEnd;
                }
                DialogueController.Instance.StartDialogue(_dialogueName, _lockPlayerInput);
                _activated = true;
            }
        }

        public void ContinueDialogue()
        {
            if (!DialogueController.Instance.CurrentDialogue)
            {
                Debug.LogWarning("There is No CurrentDialogue to continue, check if you called StartDialogue first");
                return;
            }
            if (DialogueController.Instance.CurrentDialogue.name != _dialogueName)
            {
                Debug.LogWarning($"The current dialogue: {DialogueController.Instance.CurrentDialogue.name} is different from the {_dialogueName} that the object {name} wants to continue, the command ContinueDialogue will not activate");
                return;
            }
            if (DialogueController.Instance.CurrentDialogue.name == _dialogueName) DialogueController.Instance.UpdateDialogue();
        }

        private void UnsubscribeOnDialogueStart()
        {
            DialogueController.Instance.OnDialogueStart -= _onDialogueStart.Invoke;
            DialogueController.Instance.OnDialogueEnd -= UnsubscribeOnDialogueStart;
        }

        private void UnsubscribeOnDialogueEnd()
        {
            DialogueController.Instance.OnDialogueEnd -= _onDialogueEnd.Invoke;
            DialogueController.Instance.OnDialogueEnd -= UnsubscribeOnDialogueEnd;
        }

        //public void ChangeDialogue(Dialogue dialogue)
        //{
        //    _dialogue = dialogue;
        //}

        private void OnTriggerEnter(Collider other)
        {
            StartDialogue();
            if (_deactivateObjectOnFirstActivate && _activateOnce && _activated) gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_dialogue)
            {
                _dialogueName = _dialogue.name;
            }
        }
#endif
    }
}