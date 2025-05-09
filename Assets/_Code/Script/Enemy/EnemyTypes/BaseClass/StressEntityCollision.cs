using UnityEngine;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(SphereCollider))]
    public class StressEntityCollision : MonoBehaviour
    {
        private StressEntity _stressEntity
        {
            get
            {
                if (!m_stressEntity) m_stressEntity = GetComponentInParent<StressEntity>();
                return m_stressEntity;
            }
        }
        private StressEntity m_stressEntity;
        private SphereCollider _sphereCollider;
        public SphereCollider SphereCollider
        {
            get
            {
                if (!_sphereCollider) _sphereCollider = GetComponent<SphereCollider>();
                return _sphereCollider;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            _stressEntity.EnterArea();
        }

        private void OnTriggerExit(Collider other)
        {
            _stressEntity.ExitArea();
        }
    }
}
