using System.Linq;
using UnityEngine;

namespace Ivayami.Puzzle {
    [CreateAssetMenu(menuName = "Texts/Readable")]
    public class Readable : ScriptableObject {

        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField, TextArea(1, 50)] public string Content { get; private set; }

        public Readable GetTranslation(LanguageTypes language) {
            if (language == LanguageTypes.ENUS) return this;
            Readable readable = Resources.LoadAll<Readable>($"Readable/{language}/").First(text => text.name == name);
            Resources.UnloadUnusedAssets();
            if (readable != null) return readable;
            else {
                Debug.LogError($"No translation {language} found of '{name}' (Readable)");
                return this;
            }
        }

    }
}
