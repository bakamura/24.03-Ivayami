using UnityEngine;

namespace Paranapiacaba.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private Dialogue _dialogue;
        [SerializeField] private bool _activateOnce;
        private bool _activated;
        public void StartDialogue()
        {
            if (!_activateOnce || (_activateOnce && !_activated))
            {
                DialogueController.Instance.StartDialogue(_dialogue.id);
                _activated = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            StartDialogue();
        }
    }
}