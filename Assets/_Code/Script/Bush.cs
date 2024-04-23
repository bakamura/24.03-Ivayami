using UnityEngine;
using Ivayami.Player;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(Collider))]
    public class Bush : MonoBehaviour
    {
        public static bool IsPlayerHidden { get; private set; }
        private bool _isPlayerInside;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerMovement.Instance.onCrouch.AddListener(HandleOnCrouch);
                _isPlayerInside = true;
                UpdateHiddenState();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerInside = false;
                IsPlayerHidden = false;
                PlayerMovement.Instance.onCrouch.RemoveListener(HandleOnCrouch);
                UpdateHiddenState();
            }
        }

        private void HandleOnCrouch(bool isCrouching)
        {
            UpdateHiddenState();
        }

        private void UpdateHiddenState()
        {
            IsPlayerHidden = PlayerMovement.Instance.Crouching && _isPlayerInside;
        }
    }

}