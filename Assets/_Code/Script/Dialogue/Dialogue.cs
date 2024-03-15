using UnityEngine;

namespace Paranapiacaba.Dialogue {
    [CreateAssetMenu(menuName = "DialogueSystem/Dialogue")]
    public class Dialogue : ScriptableObject {

        public string id;
        public Speech[] dialogue;
        public string onEndEventId;

    }
}