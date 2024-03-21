using UnityEngine;

namespace Paranapiacaba.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        public void StartDialogue(string dialogueID) => DialogueController.Instance.StartDialogue(dialogueID);
    }
}