using UnityEngine;
using Paranapiacaba.Player;

namespace Paranapiacaba.Prop
{
    [RequireComponent(typeof(Collider))]
    public class Bush : MonoBehaviour
    {
        public static bool IsPlayerHidden { get; private set; }
        private bool _isPlayerInside;

        private void Start()
        {
            PlayerMovement.Instance.onCrouch.AddListener(HandleOnCrouch);
        }        

        private void OnDestroy()
        {
            PlayerMovement.Instance.onCrouch.RemoveListener(HandleOnCrouch);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerInside = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerInside = false;
                IsPlayerHidden = false;
            }
        }

        private void HandleOnCrouch(bool isCrouching)
        {
            IsPlayerHidden = isCrouching && _isPlayerInside;
        }
    }

}