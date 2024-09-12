using UnityEngine;
using FMODUnity;

namespace Ivayami.Audio {
    public class MusicTrigger : MonoBehaviour {

        [SerializeField] private EventReference _music;
        [SerializeField] private bool _shouldStopPeriodicaly;

        private void OnTriggerEnter(Collider other) {
            Music.Instance.SetMusic(_music, _shouldStopPeriodicaly);
        }

        private void OnTriggerExit(Collider other) {
            Music.Instance.Stop();
        }

        private void OnDestroy() {
            Music.Instance.Stop();
        }

    }
}

