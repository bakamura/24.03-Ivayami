 using UnityEngine;
using Ivayami.Player;

namespace Ivayami.Misc
{
    public class AttachToPlayer : MonoBehaviour
    {
        [SerializeField] private bool _attachOnStart;
        [SerializeField] private Vector3 _offset;

        private void Start()
        {
            if(_attachOnStart) Attach();
        }

        public void Attach()
        {
            transform.SetParent(PlayerMovement.Instance.transform);
            transform.localPosition = _offset;
        }
    }
}