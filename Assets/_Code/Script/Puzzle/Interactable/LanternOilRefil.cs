using UnityEngine;
using Ivayami.Player.Ability;
using Ivayami.UI;
using UnityEngine.Localization;

namespace Ivayami.Puzzle
{
    public class LanternOilRefil : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _fillAmount;
        [SerializeField] private Sprite _lanternItemSprite;
        [SerializeField] private LocalizedString _fillMessageText;

        public void RefilLantern()
        {
            if (LanternRef.Instance.AbilityAquired())
            {
                LanternRef.Instance.Lantern.Fill(_fillAmount);
                InfoUpdateIndicator.Instance.DisplayUpdate(_lanternItemSprite, _fillMessageText.GetLocalizedString());
            }
        }
    }
}