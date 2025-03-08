using UnityEngine;
using Ivayami.Puzzle;

namespace Ivayami.Enemy
{
    public class PoliceOfficerDetectDoor : MonoBehaviour
    {
        private PoliceOfficer _policeOfficer;
        private void Awake()
        {
            _policeOfficer = transform.parent.GetComponent<PoliceOfficer>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.TryGetComponent<ActivableAnimation>(out ActivableAnimation door) && !door.HasBeenInteracted)
            {
                door.ForceInteract();
                _policeOfficer.OpenDoor();
            }
        }
    }
}