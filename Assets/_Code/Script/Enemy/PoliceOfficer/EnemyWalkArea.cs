using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(BoxCollider))]
    public class EnemyWalkArea : MonoBehaviour
    {
        [SerializeField] private EnemyMovementData _movementData;
        [SerializeField, Tooltip("if empty, will generate random point inside area")] private Point[] _points;
        [SerializeField] private float _delayToNextPoint;

#if UNITY_EDITOR
        [SerializeField] private bool _debugDraw;
        [SerializeField, Min(0f)] private float _gizmoSize;
        [SerializeField] private Color _debugColor = Color.red;
#endif

        private Dictionary<int, byte> _enemiesCurrentPathPointDic;
        private BoxCollider m_boxCollider;
        private BoxCollider _boxCollider
        {
            get
            {
                if (!m_boxCollider) m_boxCollider = GetComponent<BoxCollider>();
                return m_boxCollider;
            }
        }

        [System.Serializable]
        public struct Point
        {
            public Vector3 Position;
            public float DelayToNextPoint;
            public static Point EmptyValue = new Point();

            public Point(Point point)
            {
                Position = point.Position;
                DelayToNextPoint = point.DelayToNextPoint;
            }
            public Point(Vector3 pos, float delay)
            {
                Position = pos;
                DelayToNextPoint = delay;
            }
        }

        public bool GetCurrentPoint(int id, out Point point)
        {
            if (_enemiesCurrentPathPointDic != null && _enemiesCurrentPathPointDic.ContainsKey(id))
            {
                Point temp = new Point(_points[_enemiesCurrentPathPointDic[id]]);
                temp.Position += transform.position;
                point = temp;
                return true;
            }
            point = new Point();
            return false;
        }

        public Point GoToNextPoint(int id)
        {
            if (_enemiesCurrentPathPointDic != null && _enemiesCurrentPathPointDic.ContainsKey(id))
            {
                if (_points.Length > 0)
                {
                    _enemiesCurrentPathPointDic[id]++;
                    if (_enemiesCurrentPathPointDic[id] >= _points.Length) _enemiesCurrentPathPointDic[id] = 0;
                    return _points[_enemiesCurrentPathPointDic[id]];
                }
                else
                {
                    Vector3 pos = new Vector3(transform.position.x + Random.Range(-_boxCollider.bounds.extents.x, _boxCollider.bounds.extents.x),
                         transform.position.y,
                         transform.position.z + Random.Range(-_boxCollider.bounds.extents.z, _boxCollider.bounds.extents.z));
                    Point temp = new Point(pos, _delayToNextPoint);
                    return temp;
                }
            }
            return new Point();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IEnemyWalkArea>(out IEnemyWalkArea temp))
            {
                if (_enemiesCurrentPathPointDic == null) _enemiesCurrentPathPointDic = new Dictionary<int, byte>();
                temp.SetMovementData(_movementData);
                temp.SetWalkArea(this);                
                _enemiesCurrentPathPointDic.Add(other.gameObject.GetInstanceID(), 0);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IEnemyWalkArea>(out IEnemyWalkArea temp))
            {
                if (_enemiesCurrentPathPointDic == null) _enemiesCurrentPathPointDic = new Dictionary<int, byte>();
                temp.SetWalkArea(null);
                _enemiesCurrentPathPointDic.Remove(other.gameObject.GetInstanceID());
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_debugDraw)
            {
                Gizmos.color = _debugColor;
                for (int i = 0; i < _points.Length; i++)
                {
                    Gizmos.DrawSphere(transform.position + _points[i].Position, _gizmoSize);
                    Handles.Label(transform.position + _points[i].Position + new Vector3(0, _gizmoSize * 2, 0), i.ToString());
                    for (int a = 0; a < _points.Length - 1; a++)
                    {
                        Gizmos.DrawLine(transform.position + _points[i].Position, transform.position + _points[Mathf.Clamp(a + 1, 0, _points.Length - 1)].Position);
                    }
                }
            }
        }
#endif
    }
}