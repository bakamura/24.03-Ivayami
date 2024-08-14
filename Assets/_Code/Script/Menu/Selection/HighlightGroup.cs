using Ivayami.UI;
using UnityEngine;

public class HighlightGroup : MonoBehaviour {

    private Highlightable _highlitedCurrent;

    public void SetHighlightTo(Highlightable toHighlight) {
        _highlitedCurrent?.Conceal();
        _highlitedCurrent = toHighlight;
        _highlitedCurrent.Highlight();
    }

}
