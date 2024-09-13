using UnityEngine;
using Ivayami.Player;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(Collider))]
    public class Bush : MonoBehaviour
    {
        private static ushort _bushesActive;
        private bool _isPlayerInside;

        private void OnTriggerEnter(Collider other)
        {
            PlayerMovement.Instance.onCrouch.AddListener(HandleOnCrouch);
            _bushesActive++;
            _isPlayerInside = true;
            UpdateHiddenState();
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerLeave();
        }

        private void OnDisable()
        {
            if (_isPlayerInside)
            {
                PlayerLeave();
            }
        }

        private void PlayerLeave()
        {
            _isPlayerInside = false;
            _bushesActive--;
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
            else if(_bushesActive == 0) PlayerMovement.Instance.hidingState = PlayerMovement.HidingState.None;
        }
    }

}