using UnityEngine;
using Ivayami.Player;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(Collider))]
    public class Bush : MonoBehaviour
    {
        //public static bool IsPlayerHidden { get; private set; }
        private bool _isPlayerInside;

        private void OnTriggerEnter(Collider other)
        {
            PlayerMovement.Instance.onCrouch.AddListener(HandleOnCrouch);
            _isPlayerInside = true;
            UpdateHiddenState();
        }

        private void OnTriggerExit(Collider other)
        {
            _isPlayerInside = false;
            PlayerMovement.Instance.hidingState = PlayerMovement.HidingState.None;
            //IsPlayerHidden = false;
            PlayerMovement.Instance.onCrouch.RemoveListener(HandleOnCrouch);
            UpdateHiddenState();
        }

        private void HandleOnCrouch(bool isCrouching)
        {
            UpdateHiddenState();
        }

        private void UpdateHiddenState()
        {
            if (PlayerMovement.Instance.Crouching && _isPlayerInside) PlayerMovement.Instance.hidingState = PlayerMovement.HidingState.Bush;
            else PlayerMovement.Instance.hidingState = PlayerMovement.HidingState.None;
        }
    }

}