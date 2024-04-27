using UnityEngine;
using Ivayami.Player.Ability;
using Ivayami.Player;

namespace Ivayami.debug
{
    public class PlayerLearnAbilities : MonoBehaviour
    {
        void Start()
        {
            foreach (PlayerAbility ability in GetComponents<PlayerAbility>())
            {
                PlayerActions.Instance.AddAbility(ability);
            }
        }
    }
}