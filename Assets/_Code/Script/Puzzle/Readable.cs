using UnityEngine;

namespace Ivayami.Puzzle {
    [CreateAssetMenu(menuName = "Texts/Readable")]
    public class Readable : ScriptableObject {

        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField, TextArea(1, 50)] public string Content { get; private set; }

    }
}
