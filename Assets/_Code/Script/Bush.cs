using UnityEngine;
using Paranapiacaba.Player;

namespace Paranapiacaba.Prop
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
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerInside = false;
                IsPlayerHidden = false;
                PlayerMovement.Instance.onCrouch.RemoveListener(HandleOnCrouch);
            }
        }

        private void HandleOnCrouch(bool isCrouching)
        {
            IsPlayerHidden = isCrouching && _isPlayerInside;
        }
    }

}