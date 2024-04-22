using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Ivayami.Player;

namespace Ivayami.Audio {
    public class EntitySound : MonoBehaviour {

        protected void PlayOneShot(EventReference sound) {
            if (!sound.IsNull) RuntimeManager.PlayOneShotAttached(sound, gameObject);
        }

    }
}