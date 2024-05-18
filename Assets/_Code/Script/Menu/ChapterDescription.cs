using UnityEngine;

namespace Ivayami.UI {
    [CreateAssetMenu(menuName = "ChapterInfo/Description")]
    public class ChapterDescription : ScriptableObject {

        [field: SerializeField] public Sprite Image { get; private set; }
        [field: SerializeField] public string Text { get; private set; }

    }
}