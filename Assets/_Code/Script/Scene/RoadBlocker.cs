using UnityEngine;
using Ivayami.Save;

public class RoadBlocker : MonoBehaviour {

    public enum State {
        Undiscovered,
        Discovered,
        Removed
    }

    [Header("Parameters")]

    private int _id;
    private State _state;

    private void Awake() {
        _state = SaveSystem.Instance.Progress.GetRoadBlockerState(_id);
        if (_state == State.Removed) gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) {
        if (_state == State.Undiscovered) {
            _state = State.Discovered;
            SaveSystem.Instance.Progress.SaveRoadBlockerState(_id, State.Discovered);
        }
    }

    public void Remove() {
        _state = State.Removed;
        SaveSystem.Instance.Progress.SaveRoadBlockerState(_id, State.Removed);
        gameObject.SetActive(false);
    }

}
