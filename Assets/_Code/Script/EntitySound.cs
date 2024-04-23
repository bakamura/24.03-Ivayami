using UnityEngine;
using FMODUnity;

namespace Paranapiacaba.Audio {
    public class EntitySound : MonoBehaviour {

        protected void PlayLocalSound(EventReference sound) {
            if (!sound.IsNull) RuntimeManager.PlayOneShot(sound, transform.position);
        }

    }
}