using UnityEngine;

namespace Paranapiacaba.Dialogue {
    [CreateAssetMenu(menuName = "DialogueSystem/Dialogue")]
    public class Dialogue : ScriptableObject {

        [HideInInspector] public string id => name;
        public Speech[] dialogue;
        public string onEndEventId;

        private bool _hasBeenInstantiated;

        private void OnValidate()
        {
            if (!_hasBeenInstantiated)
            {
                _hasBeenInstantiated = true;
                DialogueEventsInspector.UpdateDialoguesList();
            }
        }

        private void OnDestroy()
        {
            DialogueEventsInspector.UpdateDialoguesList();
        }

    }
}