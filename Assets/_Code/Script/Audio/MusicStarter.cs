using UnityEngine;
using FMODUnity;
using System.Collections;
using Ivayami.Save;

namespace Ivayami.Audio {
    public class MusicStarter : MonoBehaviour {

        [SerializeField] private EventReference _music;

        private void Awake() {
            StartCoroutine(WaitForSaveLoad());
        }

        private IEnumerator WaitForSaveLoad() {
            while(SaveSystem.Instance.Options == null) yield return null;

            Music.Instance.SetMusic(_music);
        }


        public void Stop() {
            Music.Instance.Stop();
        }

    }
}