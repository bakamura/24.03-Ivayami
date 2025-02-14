using UnityEngine;

namespace Ivayami.Player
{
    public class PlayerStressRef : MonoBehaviour
    {
        public void SetPlayerStress(float stress)
        {
            PlayerStress.Instance.SetStress(stress);
        }
    }
}