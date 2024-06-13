using UnityEngine;

namespace Ivayami.Enemy
{
    public class PoliceOfficerAttack : MonoBehaviour
    {
        private PoliceOfficer _policeOfficer
        {
            get
            {
                if (!m_policeOfficer) m_policeOfficer = GetComponentInParent<PoliceOfficer>();
                return m_policeOfficer;
            }
        }
        private PoliceOfficer m_policeOfficer;
        private void OnTriggerEnter(Collider other)
        {
            if ((1 << other.gameObject.layer) == _policeOfficer.TargetLayer) _policeOfficer.Attack();
        }
    }
}