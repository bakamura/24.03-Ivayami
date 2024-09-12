using System.Collections;
using UnityEngine;
using FMODUnity;
using Ivayami.Save;

namespace Ivayami.Audio {
    public class MusicStarter : MonoBehaviour {

        [SerializeField] private EventReference _music;
        [SerializeField] private bool _shouldStopPeriodicaly;

        private void Awake() {
            StartCoroutine(WaitForSaveLoad());
        }

        private IEnumerator WaitForSaveLoad() {
            while(SaveSystem.Instance.Options == null) yield return null;

            Music.Instance.SetMusic(_music, _shouldStopPeriodicaly);
        }


        public void Stop() {
            Music.Instance.Stop();
        }

    }
}