using System.Collections.Generic;
using UnityEngine;
using Ivayami.Puzzle;

public class InteractableDetector : MonoBehaviour {

    private List<IInteractable> _interactablesDetected = new List<IInteractable>();
    public bool onlyHeavyObjects = false;

    public List<IInteractable> InteractablesDetected {
        get {
            _interactablesDetected.RemoveAll(interactable => interactable as MonoBehaviour == null || !(interactable as MonoBehaviour).gameObject.activeSelf);
            List<IInteractable> interactables = _interactablesDetected;
            if (onlyHeavyObjects) {
                HeavyObjectPlacement tmp;
                interactables.RemoveAll(interactable => !interactable.gameObject.TryGetComponent<HeavyObjectPlacement>(out tmp));
            }
            return interactables;
        }
    }


    private void OnTriggerEnter(Collider other) {
        if (!other.isTrigger) {
            if (other.TryGetComponent(out IInteractable interactable)) _interactablesDetected.Add(interactable);
            else Debug.LogWarning($"InteractableDetector couldn't get IInteractable from {other.name}, check if layer is misatributed");
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.TryGetComponent(out IInteractable interactable) && _interactablesDetected.Contains(interactable)) _interactablesDetected.Remove(interactable);
    }

}
