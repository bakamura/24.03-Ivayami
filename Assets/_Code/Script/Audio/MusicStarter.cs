using System.Collections;
using UnityEngine;
using FMODUnity;
using Ivayami.Save;

namespace Ivayami.Audio {
    public class MusicStarter : MonoBehaviour {

        [SerializeField] private EventReference _music;

        private void Awake() {
            StartCoroutine(WaitForSaveLoad());
        }

        private IEnumerator WaitForSaveLoad() {
            while(SaveSystem.Instance.Options == null) yield return null;

            Music.Range instant = new Music.Range(0, 0.01f);
            Music.Instance.SetMusic(_music, false, false, false, false, instant, instant, instant);
        }


        public void Stop() {
            Music.Instance.Stop(_music.Guid);
        }

    }
}