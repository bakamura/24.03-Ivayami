using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(BoxCollider))]
    public class EnemyWalkArea : MonoBehaviour
    {
        [SerializeField] private EnemyMovementData _movementData;
        [SerializeField] private bool _ignoreYAxis;
        [SerializeField, Tooltip("if empty, will generate random point inside area")] private Point[] _points;
        [SerializeField] private float _delayToNextPoint;

#if UNITY_EDITOR
        [SerializeField] private bool _debugDraw;
        [SerializeField, Min(0f)] private float _gizmoSize;
        [SerializeField] private Color _debugColor = Color.red;
#endif

        private Dictionary<int, EnemyData> _enemiesCurrentPathPointDic;
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

        [System.Serializable]
        private struct EnemyData
        {
            public Point Point;
            public byte CurrentPointIndex;

            public EnemyData(Point point, byte currentPointIndex)
            {
                Point = point;
                CurrentPointIndex = currentPointIndex;
            }
        }
        public bool GetCurrentPoint(int id, out Point point)
        {
            if (_enemiesCurrentPathPointDic != null && _enemiesCurrentPathPointDic.ContainsKey(id))
            {
                Point temp = new Point(_enemiesCurrentPathPointDic[id].Point);
                temp.Position += transform.position;
                if (_ignoreYAxis) temp.Position = new Vector3(temp.Position.x, 0, temp.Position.z);
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
                    EnemyData temp = _enemiesCurrentPathPointDic[id];
                    temp.CurrentPointIndex++;
                    if (temp.CurrentPointIndex >= _points.Length) temp.CurrentPointIndex = 0;
                    temp.Point = _points[temp.CurrentPointIndex];
                    _enemiesCurrentPathPointDic[id] = temp;
                    return temp.Point;
                }
                else
                {
                    Vector3 pos = new Vector3(transform.position.x + Random.Range(-_boxCollider.bounds.extents.x, _boxCollider.bounds.extents.x),
                         _ignoreYAxis ? 0 : transform.position.y,
                         transform.position.z + Random.Range(-_boxCollider.bounds.extents.z, _boxCollider.bounds.extents.z));
                    Point temp = new Point(pos, _delayToNextPoint);
                    if(RandomPoint(temp.Position, out temp.Position))
                    {
                        _enemiesCurrentPathPointDic[id] = new EnemyData(temp, 0);
                        return temp;
                    }
                }
            }
            return new Point();
        }

        private bool RandomPoint(Vector3 center, out Vector3 result)
        {
            for (int i = 0; i < 30; i++)
            {
                if (NavMesh.SamplePosition(center, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }
            result = Vector3.zero;
            return false;
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IEnemyWalkArea>(out IEnemyWalkArea temp))
            {
                if (_enemiesCurrentPathPointDic == null) _enemiesCurrentPathPointDic = new Dictionary<int, EnemyData>();
                temp.SetMovementData(_movementData);
                temp.SetWalkArea(this);
                _enemiesCurrentPathPointDic.Add(other.gameObject.GetInstanceID(), new EnemyData(_points[0], 0));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IEnemyWalkArea>(out IEnemyWalkArea temp))
            {
                if (_enemiesCurrentPathPointDic == null) _enemiesCurrentPathPointDic = new Dictionary<int, EnemyData>();
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