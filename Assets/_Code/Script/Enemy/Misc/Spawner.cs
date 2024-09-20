using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ivayami.Enemy
{
    public class Spawner : MonoBehaviour
    {
        [Header("Parameters")]
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField, Min(0f)] private float _spawnDelay;
        [SerializeField] private byte _spawnAmount;
        [SerializeField] private byte _maxSpawns = 1;
        [SerializeField] private bool _startActive;
        [SerializeField] private bool _animateOnSpaw;
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _drawGizmos;
        [SerializeField] private Color _gizmosColor;
        [SerializeField, Min(0f)] private float _gizmoSize;
#endif

        private readonly List<Transform> _spawnPoints = new List<Transform>();
        private WaitForSeconds _spawnDelayTick;
        private readonly List<GameObject> _currentSpawns = new List<GameObject>();

        public bool IsActive { get; private set; }

        private void Awake()
        {
            foreach (Transform transform in GetComponentsInChildren<Transform>())
            {
                if (transform.GetComponents<Component>().Length == 1) _spawnPoints.Add(transform);
            }
            _spawnDelayTick = new WaitForSeconds(_spawnDelay);
            if (_startActive) StartSpawn();
        }
        [ContextMenu("Start")]
        public void StartSpawn()
        {
            if (!IsActive && _currentSpawns.Count < _maxSpawns)
            {
                IsActive = true;
                StartCoroutine(SpawnCoroutine());
            }
        }
        [ContextMenu("Stop")]
        public void StopSpawn()
        {
            if (IsActive)
            {
                IsActive = false;
                StopCoroutine(SpawnCoroutine());
            }
        }

        private IEnumerator SpawnCoroutine()
        {
            byte currentSpawnPointIndex = 0;
            while (_currentSpawns.Count < _maxSpawns)
            {
                for (int i = 0; i < _spawnAmount; i++)
                {
                    GameObject gobj = Instantiate(_enemyPrefab, _spawnPoints[currentSpawnPointIndex].position, _spawnPoints[currentSpawnPointIndex].rotation, null);
                    _currentSpawns.Add(gobj);
                    if (_animateOnSpaw) gobj.GetComponentInChildren<EnemyAnimator>().Spawning();
                    currentSpawnPointIndex++;
                    ClampValue(ref currentSpawnPointIndex, _spawnPoints.Count);
                }
                currentSpawnPointIndex++;
                ClampValue(ref currentSpawnPointIndex, _spawnPoints.Count);
                yield return _spawnDelayTick;
            }
            IsActive = false;
        }

        private void ClampValue(ref byte value, int max)
        {
            if (value >= max) value = 0;
        }

        private void OnValidate()
        {
            if (_animateOnSpaw && _enemyPrefab && !_enemyPrefab.GetComponentInChildren<EnemyAnimator>()) _enemyPrefab = null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_drawGizmos)
            {
                Gizmos.color = _gizmosColor;
                foreach (Transform transform in GetComponentsInChildren<Transform>())
                {
                    if(transform.GetComponents<Component>().Length == 1)
                    {
                        Gizmos.DrawSphere(transform.position, _gizmoSize);
                    }
                }
            }
        }
#endif
    }
}