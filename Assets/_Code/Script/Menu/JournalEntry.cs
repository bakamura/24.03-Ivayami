using UnityEngine;

namespace Ivayami.UI {
    [CreateAssetMenu(menuName = "ChapterInfo/Description")]
    public class JournalEntry : ScriptableObject {

        [field: SerializeField] public int TemplateID { get; private set; }
        [field: SerializeField] public string Text { get; private set; }
        [field: SerializeField] public Sprite[] Images { get; private set; }

    }
}