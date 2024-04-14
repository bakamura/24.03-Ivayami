using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Paranapiacaba.Player;

namespace Paranapiacaba.Audio {
    public class EntitySound : MonoBehaviour {

        protected void PlayOneShot(EventReference sound) {
            if (!sound.IsNull) RuntimeManager.PlayOneShot(sound, transform.position);
        }

    }
}