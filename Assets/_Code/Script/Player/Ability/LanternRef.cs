using UnityEngine;

namespace Ivayami.Player.Ability
{
    [RequireComponent(typeof(Lantern))]
    public class LanternRef : MonoSingleton<LanternRef>
    {
        public Lantern Lantern { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Lantern = GetComponent<Lantern>();
        }

        public bool AbilityAquired()
        {
            return PlayerActions.Instance.CheckAbility(typeof(Lantern), out _);
        }
    }
}