using UnityEngine;

namespace Ivayami.Audio
{
    [RequireComponent(typeof(PlayerSounds))]
    public class PlayerSoundsRef : MonoSingleton<PlayerSoundsRef>
    {
        public PlayerSounds PlayerSounds { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            PlayerSounds = GetComponent<PlayerSounds>();
        }
    }
}