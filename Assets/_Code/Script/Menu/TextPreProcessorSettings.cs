using UnityEngine;

namespace Ivayami.UI {
    [CreateAssetMenu(menuName = "Ivayami/UI/TextsPreProcessorSettings")]
    public class TextPreProcessorSettings : ScriptableObject {

        [field: SerializeField] public TextTag[] TextTags { get; private set; }

    }

    [System.Serializable]
    public struct TextTag {
        public string name;
        public Color color;
    }
}
