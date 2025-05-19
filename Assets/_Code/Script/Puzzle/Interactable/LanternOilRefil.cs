using UnityEngine;
using Ivayami.Player.Ability;
using Ivayami.Player;

namespace Ivayami.Puzzle
{
    public class LanternOilRefil : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _fillAmount;
        private const string _abilityName = "Lantern";

        public void RefilLantern()
        {
            if (PlayerActions.Instance.CheckAbility(_abilityName, out PlayerAbility ability))
            {
                Lantern lantern = (Lantern)ability;
                lantern.Fill(_fillAmount);
            }
        }
    }
}