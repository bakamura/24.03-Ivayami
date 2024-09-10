using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Ivayami.Audio
{
    public class EntitySound : MonoBehaviour
    {

        [System.Serializable]
        public struct Range : System.IEquatable<Range>
        {
            [Min(0f)] public float Min;
            [Min(0f)] public float Max;

            public static Range Empty = new Range();

            public bool Equals(Range other)
            {
                if (ReferenceEquals(this, other)) return true;
                return this.Min == other.Min && this.Max == other.Max;
            }
        }

        protected void PlayOneShot(EventInstance sound, bool fadeOut, Range attenuation, EVENT_CALLBACK onAudioEnd = null)
        {
            sound.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING) sound.stop(fadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
            //sound.setVolume(SaveSystem.Instance && SaveSystem.Instance.Options != null ? SaveSystem.Instance.Options.sfxVol : 1f);

            sound.getDescription(out EventDescription description);
            description.is3D(out bool is3d);
            if (is3d)
            {
                RuntimeManager.AttachInstanceToGameObject(sound, transform);
                if (!attenuation.Equals(Range.Empty))
                {
                    sound.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, attenuation.Min);
                    sound.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, attenuation.Max);
                }
            }

            if (onAudioEnd != null) sound.setCallback(onAudioEnd, EVENT_CALLBACK_TYPE.SOUND_STOPPED);
            sound.start();
            //RuntimeManager.PlayOneShotAttached(new EventReference(), gameObject);
        }


        protected EventInstance InstantiateEvent(EventReference sound)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(sound);
            return eventInstance;
        }

    }
}