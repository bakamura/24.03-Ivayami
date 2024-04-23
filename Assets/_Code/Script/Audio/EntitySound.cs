using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Ivayami.Save;

namespace Ivayami.Audio {
    public class EntitySound : MonoBehaviour {

        protected void PlayOneShot(EventInstance sound) {
            sound.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING) sound.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            sound.setVolume(SaveSystem.Instance.Options.sfxVol);
            sound.start();
            //RuntimeManager.PlayOneShotAttached(new EventReference(), gameObject);
        }

        protected EventInstance InstantiateEvent(EventReference sound) {
            EventInstance eventInstance = RuntimeManager.CreateInstance(sound);
            return eventInstance;
        }

    }
}