using UnityEngine;

namespace Ivayami.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private Dialogue _dialogue;
        [SerializeField, ReadOnly] private string _dialogueName;
        [SerializeField] private bool _activateOnce;
        [SerializeField] private bool _lockPlayerInput;
        private bool _activated;

        private void Start()
        {
            if (_dialogue) Resources.UnloadAsset(_dialogue);
        }

        [ContextMenu("StartDialogue")]
        public void StartDialogue()
        {
            if (!_activateOnce || (_activateOnce && !_activated))
            {
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

        //public void ChangeDialogue(Dialogue dialogue)
        //{
        //    _dialogue = dialogue;
        //}

        private void OnTriggerEnter(Collider other)
        {
            StartDialogue();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_dialogue)
            {
                _dialogueName = _dialogue.name;
                _dialogue = null;
            }
        }
#endif
    }
}