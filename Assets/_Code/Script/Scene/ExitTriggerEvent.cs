using System.Collections;
using UnityEngine;
using Ivayami.Puzzle;

namespace Ivayami.Scene {
    public class ExitTriggerEvent : TriggerEvent {

        [SerializeField, Range(1, 10)] private int _framesUntilDeactivate;

        private void Awake() {
            StartCoroutine(FramesUntillDeactivate(_framesUntilDeactivate));
        }

        protected override void OnTriggerEnter(Collider other) {
            base.OnTriggerEnter(other);

            gameObject.SetActive(false);
        }

        private IEnumerator FramesUntillDeactivate(int framesUntilDeactivate) {
            for(int i = 0; i < framesUntilDeactivate; i++) yield return null;

            gameObject.SetActive(false);
        }

    }
}
