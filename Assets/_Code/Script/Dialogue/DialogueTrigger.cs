using UnityEngine;

namespace Ivayami.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private Dialogue _dialogue;
        [SerializeField] private bool _activateOnce;
        [SerializeField] private bool _lockPlayerInput;
        private bool _activated;
        private int _dialogueId;

        private void Awake()
        {
            _dialogueId = _dialogue.ID;
        }

        [ContextMenu("StartDialogue")]
        public void StartDialogue()
        {
            if (!_activateOnce || (_activateOnce && !_activated))
            {
                DialogueController.Instance.StartDialogue(_dialogueId, _lockPlayerInput);
                _activated = true;
            }
        }

        public void ContinueDialogue()
        {
            if(!DialogueController.Instance.CurrentDialogue)
            {
                Debug.LogWarning("There is No CurrentDialogue to continue, check if you called StartDialogue first");
                return;
            }
            if(DialogueController.Instance.CurrentDialogue.ID != _dialogueId)
            {
                Debug.LogWarning($"The current dialogue: {DialogueController.Instance.CurrentDialogue.ID} is different from the {_dialogue.ID} that the object {name} wants to continue, the command ContinueDialogue will not activate");
                return;
            }
            if (DialogueController.Instance.CurrentDialogue.ID == _dialogueId) DialogueController.Instance.UpdateDialogue();
        }

        public void ChangeDialogue(Dialogue dialogue)
        {
            _dialogue = dialogue;
        }

        private void OnTriggerEnter(Collider other)
        {
            StartDialogue();
        }
    }
}