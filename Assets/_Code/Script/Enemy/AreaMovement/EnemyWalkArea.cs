using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using System;
using UnityEngine.Events;
#if UNITY_EDITOR
using System.Linq;
#endif

namespace Ivayami.Enemy
{
    //[RequireComponent(typeof(BoxCollider))]
    public class EnemyWalkArea : MonoBehaviour
    {
        [SerializeField] private EnemyMovementData _movementData;
        //[SerializeField] private bool _ignoreYAxis;
        [SerializeField] private PatrolBehaviour _patrolBehaviour;
        [SerializeField, Tooltip("if empty, will generate random point inside area")] private Point[] _points;
        [SerializeField] private float _delayToNextPoint;

        [Serializable]
        private enum PatrolBehaviour
        {
            RandomPoints,
            Loop,
            BackAndForth
        }
        [Serializable]
        public struct PathCallback
        {
            [Tooltip("Use the same number shown in the Element text in the Point array")] public byte PointIndex;
            public UnityEvent OnPointReached;
        }
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
        private bool _pointsInitialized;
        public EnemyMovementData MovementData => _movementData;

        [Serializable]
        public struct Point
        {
            public Vector3 Position;
            public float DelayToNextPoint;
            //[HideInInspector] public byte PointIndex;
            //public static Point EmptyValue = new Point();

            public Point(Point point)
            {
                Position = point.Position;
                DelayToNextPoint = point.DelayToNextPoint;
                //PointIndex = 0;
            }
            public Point(Vector3 pos, float delay, byte index)
            {
                Position = pos;
                DelayToNextPoint = delay;
                //PointIndex = index;
            }
        }

        [Serializable]
        public struct EnemyData
        {
            public Point Point;
            public sbyte CurrentPointIndex;
            public sbyte CurrentDirection;
#if UNITY_EDITOR
            public string EnemyName;
#endif

            public EnemyData(Point point, sbyte currentDirection, sbyte currentPointIndex)
            {
                Point = point;
                CurrentPointIndex = currentPointIndex;
                CurrentDirection = currentDirection;
#if UNITY_EDITOR
                EnemyName = null;
#endif
            }

#if UNITY_EDITOR
            public EnemyData(Point point, sbyte currentDirection, sbyte currentPointIndex, string enemyName)
            {
                Point = point;
                CurrentPointIndex = currentPointIndex;
                CurrentDirection = currentDirection;
                EnemyName = enemyName;
            }
#endif
        }
        public bool GetCurrentPoint(int id, out EnemyData point)
        {
            if (_enemiesCurrentPathPointDic.ContainsKey(id))
            {
                //Point temp = new Point(_enemiesCurrentPathPointDic[id].Point);
                //temp.Position += transform.position;
                //if (_ignoreYAxis) temp.Position = new Vector3(temp.Position.x, 0, temp.Position.z);
                point = _enemiesCurrentPathPointDic[id];
                return true;
            }
            point = new EnemyData();
            return false;
        }

        public EnemyData GoToNextPoint(int id)
        {
            if (_enemiesCurrentPathPointDic.ContainsKey(id))
            {
                EnemyData temp;
                switch (_patrolBehaviour)
                {
                    case PatrolBehaviour.RandomPoints:
#if UNITY_EDITOR
                        _enemiesCurrentPathPointDic[id] = new EnemyData(GenerateRandomPointInsideArea(), 1, 0, _enemiesCurrentPathPointDic[id].EnemyName);
                        return _enemiesCurrentPathPointDic[id];
#else
                        _enemiesCurrentPathPointDic[id] = new EnemyData(GenerateRandomPointInsideArea(), 1, 0);
                        return _enemiesCurrentPathPointDic[id];
#endif
                    case PatrolBehaviour.Loop:
                        temp = _enemiesCurrentPathPointDic[id];
                        temp.CurrentPointIndex++;
                        if (temp.CurrentPointIndex >= _points.Length) temp.CurrentPointIndex = 0;
                        temp.Point = _points[temp.CurrentPointIndex];
                        temp.Point.Position += transform.position;
                        //if (_ignoreYAxis) temp.Point.Position = new Vector3(temp.Point.Position.x, 0, temp.Point.Position.z);
                        _enemiesCurrentPathPointDic[id] = temp;
                        return _enemiesCurrentPathPointDic[id];
                    case PatrolBehaviour.BackAndForth:
                        temp = _enemiesCurrentPathPointDic[id];
                        if (temp.CurrentPointIndex + temp.CurrentDirection >= _points.Length) temp.CurrentDirection = -1;
                        else if (temp.CurrentPointIndex + temp.CurrentDirection < 0) temp.CurrentDirection = 1;
                        temp.CurrentPointIndex += temp.CurrentDirection;
                        temp.Point = _points[temp.CurrentPointIndex];
                        temp.Point.Position += transform.position;
                        //if (_ignoreYAxis) temp.Point.Position = new Vector3(temp.Point.Position.x, 0, temp.Point.Position.z);
                        _enemiesCurrentPathPointDic[id] = temp;
                        return _enemiesCurrentPathPointDic[id];
                }
            }
            return new EnemyData();
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
            Vector3 pos = new Vector3(transform.position.x + UnityEngine.Random.Range(-_boxCollider.bounds.extents.x, _boxCollider.bounds.extents.x),
                         /*_ignoreYAxis ? 0 : */transform.position.y,
                         transform.position.z + UnityEngine.Random.Range(-_boxCollider.bounds.extents.z, _boxCollider.bounds.extents.z));
            Point temp = new Point(pos, _delayToNextPoint, 0);
            if (TryGetPointInNavMeshArea(temp.Position, out temp.Position))
            {
                return temp;
            }
            return new Point(transform.position, _delayToNextPoint, 0);
        }

        //private void InitializePointsIndex()
        //{
        //    if (_pointsInitialized) return;
        //    for (byte i = 0; i < _points.Length; i++)
        //    {
        //        _points[i].PointIndex = i;
        //    }
        //    _pointsInitialized = true;
        //}

        public void AddEnemyToArea(IEnemyWalkArea enemy, string enemyName)
        {
            //InitializePointsIndex();
            if (!_enemiesCurrentPathPointDic.ContainsKey(enemy.ID))
            {
                if (enemy.CanChangeWalkArea)
                {
                    enemy.SetWalkArea(this);
                    if (_movementData) enemy.SetMovementData(_movementData);
                }
                EnemyData data = new EnemyData(new Point(), 1 ,-1);
                _enemiesCurrentPathPointDic.Add(enemy.ID, data);
                data = GoToNextPoint(enemy.ID);
#if UNITY_EDITOR
                data.EnemyName = enemyName;
#endif
                _enemiesCurrentPathPointDic[enemy.ID] = data;
            }
        }

        public void RemoveEnemyFromArea(IEnemyWalkArea enemy)
        {
            if (_enemiesCurrentPathPointDic.ContainsKey(enemy.ID) && enemy.CanChangeWalkArea)
            {
                if(enemy.CanChangeWalkArea) enemy.SetWalkArea(null);
                _enemiesCurrentPathPointDic.Remove(enemy.ID);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IEnemyWalkArea>(out IEnemyWalkArea temp))
            {                
                AddEnemyToArea(temp, other.gameObject.name);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IEnemyWalkArea>(out IEnemyWalkArea temp))
            {
                RemoveEnemyFromArea(temp);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_debugDraw)
            {
                Gizmos.color = _debugColor;
                if (_patrolBehaviour != PatrolBehaviour.RandomPoints)
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
                    for (int i = 0; i < _enemiesCurrentPathPointDic.Count; i++)
                    {
                        EnemyData[] temp = _enemiesCurrentPathPointDic.Values.ToArray();
                        Gizmos.DrawSphere(temp[i].Point.Position, _gizmoSize);
                        Handles.Label(temp[i].Point.Position + new Vector3(0, _gizmoSize * 4, 0), temp[i].EnemyName);
                    }
                }
            }
        }
#endif
    }
}