using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Paranapiacaba.Player.Ability;

namespace Paranapiacaba.Player {
    public class PlayerActions : MonoSingleton<PlayerActions> {

        public UnityEvent<byte> onAbilityChange = new UnityEvent<byte>();
        [SerializeField] private float _interactionQuickDuration;
        private List<PlayerAbility> _abilities;

    }
}