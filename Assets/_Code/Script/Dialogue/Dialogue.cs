using UnityEngine;

namespace Paranapiacaba.Dialogue {
    [CreateAssetMenu(menuName = "DialogueSystem/Dialogue")]
    public class Dialogue : ScriptableObject {

        public Speech[] dialogue;
        public string onEndEventId;

    }
}