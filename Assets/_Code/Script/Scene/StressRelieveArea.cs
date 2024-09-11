using System.Collections;
using UnityEngine;
using Ivayami.Player;

namespace Ivayami.Scene {
    public class StressRelieveArea : MonoBehaviour {

        [Header("Parameters")]

        [SerializeField, Min(0f)] private float _stressRelieveRate;
        private Coroutine _stressRelieveRoutine;
        [SerializeField, Min(0f] private float _routineTickRate;

        private void OnTriggerEnter(Collider other) {
            _stressRelieveRoutine = StartCoroutine(RelieveRoutine());
        }

        private void OnTriggerExit(Collider other) {
            if (_stressRelieveRoutine != null) StopCoroutine(_stressRelieveRoutine);
        }

        private void OnDestroy() {
            if (_stressRelieveRoutine != null) StopCoroutine(_stressRelieveRoutine);
        }

        private IEnumerator RelieveRoutine() {
            WaitForSeconds tick = new WaitForSeconds(_routineTickRate);
            float relieveTick = -_stressRelieveRate / _routineTickRate;
            while (true) {
                PlayerStress.Instance.AddStress(relieveTick * Time.deltaTime);

                yield return tick;
            }

        }

    }
}
