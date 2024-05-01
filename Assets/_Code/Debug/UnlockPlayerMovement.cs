using UnityEngine;
using Ivayami.Player;

namespace Ivayami.debug
{
    public class UnlockPlayerMovement : MonoBehaviour
    {
        private void Start()
        {
            PlayerMovement.Instance.ToggleMovement(true);
            PlayerActions.Instance.ChangeInputMap("Player");
        }
    }
}