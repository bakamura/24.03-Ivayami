using System.Collections.Generic;
using UnityEngine;
using Ivayami.Puzzle;
using Ivayami.Player;

public class InteractableDetector : MonoBehaviour {

    [SerializeField] private float _heightMax;
    [SerializeField] private float _heightMin;

    [SerializeField, Range(0f, 1f)] private float _camCapMax;
    [SerializeField, Range(0f, 1f)] private float _camCapMin;

    private List<IInteractable> _interactablesDetected = new List<IInteractable>();
    [HideInInspector] public bool onlyHeavyObjects = false;

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
        transform.localPosition = Mathf.Lerp(_heightMax, _heightMin, ScaleRange(PlayerCamera.Instance.FreeLookCam.m_YAxis.Value)) * Vector3.up;
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

    private float ScaleRange(float value) {
        if (value < _camCapMin) value = _camCapMin;
        else if (value > _camCapMax) value = _camCapMax;
        return (value - _camCapMin) / (_camCapMax - _camCapMin);
    }

#if UNITY_EDITOR
    private void OnValidate() {
        if (_camCapMax < _camCapMin) {
            _camCapMax = _camCapMin;
            Debug.LogWarning($"InteractableDetector max height is lower than min, setting it to min");
        }
    }
#endif

}
