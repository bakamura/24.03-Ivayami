using UnityEngine;

namespace Ivayami.Dialogue {
    [CreateAssetMenu(menuName = "DialogueSystem/Dialogue")]
    public class Dialogue : ScriptableObject {

        [HideInInspector] public string id => name;
        public Speech[] dialogue;
        public string onEndEventId;

#if UNITY_EDITOR
        private bool _hasBeenInstantiated;
        private int _previousSize;
        private void OnValidate()
        {
            if (!_hasBeenInstantiated)
            {
                _hasBeenInstantiated = true;
                DialogueEventsInspector.UpdateDialoguesList();
            }
            if(_previousSize > 0 && _previousSize < dialogue.Length)
            {
                dialogue[dialogue.Length - 1].eventId = null;
                dialogue[dialogue.Length - 1].FilterTags = null;
                dialogue[dialogue.Length - 1].FixedDurationInSpeech = 0;
            }
            _previousSize = dialogue.Length;
        }

        private void OnDestroy()
        {
            DialogueEventsInspector.UpdateDialoguesList();
        }
#endif
    }
}