using UnityEngine;
using FMODUnity;

namespace Ivayami.Audio {
    public class MusicTrigger : MonoBehaviour {

        [SerializeField] private EventReference _music;

        private void OnTriggerEnter(Collider other) {
            Music.Instance.SetMusic(_music);
        }

        private void OnTriggerExit(Collider other) {
            Music.Instance.Stop();
        }

    }
}

