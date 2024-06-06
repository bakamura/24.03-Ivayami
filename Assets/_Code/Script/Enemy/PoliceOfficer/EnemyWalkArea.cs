using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(BoxCollider))]
    public class EnemyWalkArea : MonoBehaviour
    {
        [SerializeField, Tooltip("if empty, will generate random point inside area")] private Point[] _points;
        [SerializeField] private float _delayToNextPoint;

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

        public Point GetCurrentPoint(int id)
        {
            Point temp = new Point(_points[_enemiesCurrentPathPointDic[id]]);
            temp.Position += transform.position;
            return temp;
        }

        public Point GoToNextPoint(int id)
        {
            if(_points.Length > 0)
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

        private void OnTriggerEnter(Collider other)
        {            
            if(other.TryGetComponent<IEnemyWalkArea>(out IEnemyWalkArea temp))
            {
                if (_enemiesCurrentPathPointDic == null) _enemiesCurrentPathPointDic = new Dictionary<int, byte>();
                temp.CurrentWalkArea = this;
                _enemiesCurrentPathPointDic.Add(other.gameObject.GetInstanceID(), 0);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IEnemyWalkArea>(out IEnemyWalkArea temp))
            {
                if (_enemiesCurrentPathPointDic == null) _enemiesCurrentPathPointDic = new Dictionary<int, byte>();
                temp.CurrentWalkArea = null;
                _enemiesCurrentPathPointDic.Remove(other.gameObject.GetInstanceID());
            }
        }
    }
}