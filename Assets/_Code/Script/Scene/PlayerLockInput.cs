using UnityEngine;
using Ivayami.Player;
using Ivayami.UI;

namespace Ivayami.Scene
{
    public class PlayerLockInput : MonoBehaviour 
    {
        //[SerializeField] private string _blockKey;

        public void LockInput()
        {
            //PlayerMovement.Instance.ToggleMovement(_blockKey, false);
            Pause.Instance.canPause = false;
            PlayerActions.Instance.ChangeInputMap(null);
        }

        public void UnlockInput()
        {
            //PlayerMovement.Instance.ToggleMovement(_blockKey, true);
            Pause.Instance.canPause = true;
            PlayerActions.Instance.ChangeInputMap("Player");
        }        

        public void ChangeInputToUI()
        {
            PlayerActions.Instance.ChangeInputMap("Menu");
        }
    }
}