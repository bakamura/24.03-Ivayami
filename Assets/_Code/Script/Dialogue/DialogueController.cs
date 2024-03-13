using UnityEngine;

namespace Paranapiacaba.Dialogue {
    public class DialogueController : MonoSingleton<DialogueController> {

        [SerializeField] private float _characterShowDelay;
        [SerializeField] private Dialogue[] _dialogues;

        public void StartDialogue(string dialogueId) {
            Debug.LogWarning("Method Not Implemented Yet");
        }

    }
}