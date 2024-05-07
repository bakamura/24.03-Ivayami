using UnityEngine;

namespace Ivayami.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private Dialogue _dialogue;
        [SerializeField] private bool _activateOnce;
        [SerializeField] private bool _lockPlayerInput;
        private bool _activated;
        public void StartDialogue()
        {
            if (!_activateOnce || (_activateOnce && !_activated))
            {
                DialogueController.Instance.StartDialogue(_dialogue.id, _lockPlayerInput);
                _activated = true;
            }
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