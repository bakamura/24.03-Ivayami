using UnityEngine;

namespace Ivayami.UI {
    public class CompositeHighlightable : Highlightable {

        [Header("References")]

        [SerializeField] private Highlightable[] _highlightables;

        public override void Highlight() {
            foreach(Highlightable highlightable in _highlightables) highlightable.Highlight();
        }

        public override void Conceal() {
            foreach(Highlightable highlightable in _highlightables) highlightable.Conceal();
        }

    }
}
