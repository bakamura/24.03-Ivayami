using UnityEngine;
using Ivayami.Player.Ability;
using Ivayami.Player;

namespace Ivayami.Puzzle
{
    public class LanternOilRefil : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _fillAmount;

        public void RefilLantern()
        {
            if (PlayerActions.Instance.CheckAbility(typeof(Lantern), out PlayerAbility ability))
            {
                Lantern lantern = (Lantern)ability;
                lantern.Fill(_fillAmount);
            }
        }
    }
}