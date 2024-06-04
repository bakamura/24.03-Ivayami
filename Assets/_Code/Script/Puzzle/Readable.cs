using UnityEngine;

namespace Ivayami.Puzzle {
    [CreateAssetMenu(menuName = "Item/Readable")]
    public class Readable : ScriptableObject {

        [field: SerializeField, TextArea(1, 50)] public string Content { get; private set; }

    }
}
