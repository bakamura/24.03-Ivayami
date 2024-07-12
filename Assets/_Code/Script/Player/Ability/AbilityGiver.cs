using UnityEngine;

namespace Ivayami.Player.Ability {
    public class AbilityGiver : MonoBehaviour {

        [SerializeField] private PlayerAbility _ability;

        public void GiveAbility() {
            PlayerActions.Instance.AddAbility(_ability);
        }

        public void RemoveAbility() {
            PlayerActions.Instance.RemoveAbility(_ability);
        }

    }
}
