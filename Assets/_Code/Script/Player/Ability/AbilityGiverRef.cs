using UnityEngine;

namespace Ivayami.Player.Ability {
    public class AbilityGiverRef : MonoBehaviour {

        public void GiveAbility(string abilityName) {
            foreach(AbilityGiver giver in FindObjectsOfType<AbilityGiver>()) {
                if (giver.Ability.GetType().Name == abilityName) {
                    giver.GiveAbility();

                    break;
                }
            }
        }

    }
}
