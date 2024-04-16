using UnityEngine;

namespace Paranapiacaba.UI {
    [CreateAssetMenu(menuName = "ChapterInfo/Description")]
    public class ChapterDescription : ScriptableObject {

        [SerializeField] private Sprite _image;
        [SerializeField] private string _text;

        public Sprite Image { get { return _image; } }
        public string Text { get { return _text; } }

    }
}