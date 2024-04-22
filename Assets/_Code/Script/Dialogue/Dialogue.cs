using UnityEngine;

namespace Ivayami.Dialogue {
    [CreateAssetMenu(menuName = "DialogueSystem/Dialogue")]
    public class Dialogue : ScriptableObject {

        [HideInInspector] public string id => name;
        public Speech[] dialogue;
        public string onEndEventId;

        private bool _hasBeenInstantiated;

#if UNITY_EDITOR
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
#endif
    }
}