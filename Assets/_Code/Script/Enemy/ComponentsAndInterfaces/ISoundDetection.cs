using UnityEngine;

namespace Ivayami.Enemy
{
    public interface ISoundDetection
    {
        public Vector3 CurrentPosition { get; }
        public abstract void GoToSoundPosition(Vector3 target/*, float speedIncrease, float durationInPlace*/);
    }
}