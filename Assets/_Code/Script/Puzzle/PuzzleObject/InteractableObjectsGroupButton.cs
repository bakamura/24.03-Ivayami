using UnityEngine;
using UnityEngine.UI;
using Ivayami.UI;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(Button), typeof(ButtonEventsCustom))]
    public class InteractableObjectsGroupButton : MonoBehaviour
    {
        [HideInInspector] public Button Button;
        public IInteractable Interactable;

        public void Setup(IInteractable interactable, InteractableObjectsGroup puzzleGroup)
        {
            Button = GetComponent<Button>();
            Interactable = interactable;
            Button.onClick.AddListener(() =>
            {
                puzzleGroup.InteractableSelected();
                Interactable.Interact();
            });
            ButtonEventsCustom events = GetComponent<ButtonEventsCustom>();
            events.OnSelectSelectable.AddListener((call) => puzzleGroup.SetCurrentSelected(this));
            events.OnPointerEnterSelectable.AddListener((call) => puzzleGroup.SetCurrentSelected(this));
        }
    }
}