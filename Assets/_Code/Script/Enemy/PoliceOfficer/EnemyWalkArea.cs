using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using System.Linq;
#endif

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(BoxCollider))]
    public class EnemyWalkArea : MonoBehaviour
    {
        [SerializeField] private EnemyMovementData _movementData;
        //[SerializeField] private bool _ignoreYAxis;
        [SerializeField, Tooltip("if empty, will generate random point inside area")] private Point[] _points;
        [SerializeField] private float _delayToNextPoint;

#if UNITY_EDITOR
        [SerializeField] private bool _debugDraw;
        [SerializeField, Min(0f)] private float _gizmoSize;
        [SerializeField] private Color _debugColor = Color.red;
#endif

        private Dictionary<int, EnemyData> _enemiesCurrentPathPointDic = new Dictionary<int, EnemyData>();
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
            //public static Point EmptyValue = new Point();

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
#if UNITY_EDITOR
            public string EnemyName;
#endif

            public EnemyData(Point point, byte currentPointIndex)
            {
                Point = point;
                CurrentPointIndex = currentPointIndex;
#if UNITY_EDITOR
                EnemyName = null;
#endif
            }

#if UNITY_EDITOR
            public EnemyData(Point point, byte currentPointIndex, string enemyName)
            {
                Point = point;
                CurrentPointIndex = currentPointIndex;
                EnemyName = enemyName;
            }
#endif
        }
        public bool GetCurrentPoint(int id, out Point point)
        {
            if (_enemiesCurrentPathPointDic != null && _enemiesCurrentPathPointDic.ContainsKey(id))
            {
                Point temp = new Point(_enemiesCurrentPathPointDic[id].Point);
                //temp.Position += transform.position;
                //if (_ignoreYAxis) temp.Position = new Vector3(temp.Position.x, 0, temp.Position.z);
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
                    temp.Point.Position += transform.position;
                    //if (_ignoreYAxis) temp.Point.Position = new Vector3(temp.Point.Position.x, 0, temp.Point.Position.z);
                    _enemiesCurrentPathPointDic[id] = temp;
                    return temp.Point;
                }
                else
                {
                    _enemiesCurrentPathPointDic[id] = new EnemyData(GenerateRandomPointInsideArea(), 0);
                }
            }
            return new Point(transform.position, 0);
        }

        private bool TryGetPointInNavMeshArea(Vector3 center, out Vector3 result)
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

        private Point GenerateRandomPointInsideArea()
        {
            Vector3 pos = new Vector3(transform.position.x + Random.Range(-_boxCollider.bounds.extents.x, _boxCollider.bounds.extents.x),
                         /*_ignoreYAxis ? 0 : */transform.position.y,
                         transform.position.z + Random.Range(-_boxCollider.bounds.extents.z, _boxCollider.bounds.extents.z));
            Point temp = new Point(pos, _delayToNextPoint);
            if (TryGetPointInNavMeshArea(temp.Position, out temp.Position))
            {
                return temp;
            }
            return new Point(transform.position, _delayToNextPoint);
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IEnemyWalkArea>(out IEnemyWalkArea temp) && !_enemiesCurrentPathPointDic.ContainsKey(other.gameObject.GetInstanceID()))
            {
                temp.SetMovementData(_movementData);
                temp.SetWalkArea(this);
                EnemyData data = new EnemyData(_points.Length > 0 ? new Point(_points[0].Position + transform.position, _points[0].DelayToNextPoint) : GenerateRandomPointInsideArea(), 0);
#if UNITY_EDITOR
                data.EnemyName = other.gameObject.name;
#endif
                _enemiesCurrentPathPointDic.Add(other.gameObject.GetInstanceID(), data);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IEnemyWalkArea>(out IEnemyWalkArea temp) && _enemiesCurrentPathPointDic.ContainsKey(other.gameObject.GetInstanceID()))
            {
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
                if (_points.Length > 0)
                {
                    for (int i = 0; i < _points.Length; i++)
                    {
                        Gizmos.DrawSphere(transform.position + _points[i].Position, _gizmoSize);
                        Handles.Label(transform.position + _points[i].Position + new Vector3(0, _gizmoSize * 2, 0), i.ToString());
                    }
                    for (int a = 0; a < _points.Length - 1; a++)
                    {
                        Gizmos.DrawLine(transform.position + _points[a].Position, transform.position + _points[Mathf.Clamp(a + 1, 0, _points.Length - 1)].Position);
                    }
                }
                else
                {
                    if(_enemiesCurrentPathPointDic != null)
                    {
                        for (int i = 0; i < _enemiesCurrentPathPointDic.Count; i++)
                        {
                            EnemyData[] temp = _enemiesCurrentPathPointDic.Values.ToArray();
                            Gizmos.DrawSphere(temp[i].Point.Position, _gizmoSize);
                            Handles.Label(temp[i].Point.Position + new Vector3(0, _gizmoSize * 2, 0), temp[i].EnemyName);
                        }
                    }
                }
            }
        }
#endif
    }
}