using System.Collections.Generic;
using UnityEngine;
using Ivayami.Puzzle;

public class InteractableDetector : MonoBehaviour {

    public List<IInteractable> InteractablesDetected { get; private set; } = new List<IInteractable>();

    private void OnTriggerEnter(Collider other) {
        if (!other.isTrigger) {
            if (other.TryGetComponent(out IInteractable interactable)) InteractablesDetected.Add(interactable);
            else Debug.LogWarning($"InteractableDetector couldn't get IInteractable from {other.name}, check if layer is misatributed");
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.TryGetComponent(out IInteractable interactable) && InteractablesDetected.Contains(interactable)) InteractablesDetected.Remove(interactable);
    }

}
