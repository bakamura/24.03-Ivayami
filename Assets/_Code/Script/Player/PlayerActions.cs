using System.Collections.Generic;
using UnityEngine.Events;

namespace Paranapiacaba.Player {
    public class PlayerActions : MonoSingleton<PlayerActions> {

        public UnityEvent<byte> onAbilityChange = new UnityEvent<byte>();
        private List<PlayerAbility> _abilities;

    }
}