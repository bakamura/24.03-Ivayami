using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ivayami.Enemy
{
    public class EnemyLight : MonoBehaviour
    {
        [SerializeField, Min(.02f)] private float _tickFrequency;
        [SerializeField, Min(0f)] private float _range;
#if UNITY_EDITOR
        [SerializeField] private Color _debugColor;
#endif



#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _debugColor;
            Gizmos.DrawSphere(transform.position, _range);
        }
#endif
    }
}