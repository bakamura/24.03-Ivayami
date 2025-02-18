using System.Collections.Generic;
using UnityEngine;
using Ivayami.Puzzle;
using Ivayami.Player;

public class InteractableDetector : MonoBehaviour {

    [SerializeField] private float _heightMax;
    [SerializeField] private float _heightMin;

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

    private void Update() {
        transform.localPosition = Mathf.Lerp(_heightMax, _heightMin, PlayerCamera.Instance.FreeLookCam.m_YAxis.Value) * Vector3.up;
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
