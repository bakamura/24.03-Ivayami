using UnityEngine;
using Ivayami.Player;
using Ivayami.UI;

namespace Ivayami.Scene
{
    public class PlayerLockInput : MonoBehaviour
    {       
        public void LockInput()
        {
            PlayerMovement.Instance.ToggleMovement(false);
            Pause.Instance.canPause = false;
            PlayerActions.Instance.ChangeInputMap(null);
        }

        public void UnlockInput()
        {
            PlayerMovement.Instance.ToggleMovement(true);
            Pause.Instance.canPause = true;
            PlayerActions.Instance.ChangeInputMap("Player");
        }        

        public void ChangeInputToUI()
        {
            PlayerActions.Instance.ChangeInputMap("Menu");
        }
    }
}