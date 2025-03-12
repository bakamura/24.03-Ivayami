using UnityEngine;

namespace Ivayami.Player.Ability {
    public class AbilityGiver : MonoBehaviour {

        [field: SerializeField] public PlayerAbility Ability { get; private set; }

        public void GiveAbility() {
            if (!PlayerActions.Instance.CheckAbility(Ability)) PlayerActions.Instance.AddAbility(Ability);
            else Debug.LogWarning($"Trying to give Ability '{Ability.GetType().Name}' to Player, but it alrady has it!");
        }

        public void RemoveAbility() {
            PlayerActions.Instance.RemoveAbility(Ability);
        }

    }
}
