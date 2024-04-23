using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Ivayami.Player;
using Ivayami.Puzzle;

namespace Ivayami.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider))]
    public class Patrol : MonoBehaviour
    {
        [Header("Parameters")]
        [SerializeField, Min(0)] private float _minDistanceFromTarget;
        [SerializeField, Min(0)] private float _detectionRange;
        [SerializeField] private float _visionAngle;
        [SerializeField, Min(.02f)] private float _tickFrequency = .5f;
        [SerializeField, Min(0)] private float _stressIncreaseOnTargetDetected;
        [SerializeField, Min(0)] private float _stressIncreaseWhileChasing;
        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private Vector3[] _patrolPoints;

        [Header("Debug")]
        [SerializeField] private bool _debugDraw;
        [SerializeField] private Color _minDistanceAreaColor = Color.red;
        [SerializeField] private Color _detectionRangeAreaColor = Color.green;
        [SerializeField] private Color _patrolPointsColor = Color.black;
        [SerializeField, Min(0)] private float _patrolPointRadius = .2f;

        private Mesh _FOVMesh;
        private NavMeshAgent _navMeshAgent;
        private WaitForSeconds _delay;
        private CapsuleCollider _collision;
        private Vector3 _initialPosition;
        private bool _isChasing;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _collision = GetComponent<CapsuleCollider>();
            _delay = new WaitForSeconds(_tickFrequency);

            _initialPosition = transform.position;
            _navMeshAgent.stoppingDistance = _collision.radius + .2f;
        }

        private void Update()
        {
            if (_isChasing)
                PlayerStress.Instance.AddStress(_stressIncreaseWhileChasing * Time.deltaTime);
        }

        private void Start()
        {
            StartCoroutine(BehaviourCoroutine());
        }

        private IEnumerator BehaviourCoroutine()
        {
            float halfVisionAngle = _visionAngle / 2f;
            byte currentPatrolPointIndex = 0;
            sbyte indexFactor = 1;
            while (true)
            {
                if (!Bush.IsPlayerHidden && (Vector3.Distance(PlayerMovement.Instance.transform.position, transform.position) <= _minDistanceFromTarget
                    || (Physics.CheckSphere(transform.position, _detectionRange, _playerLayer)
                    && Vector3.Distance(PlayerMovement.Instance.transform.position, transform.position) <= _detectionRange
                    && Vector3.Angle(PlayerMovement.Instance.transform.position, transform.position) <= halfVisionAngle)))
                {
                    if (!_isChasing)
                    {
                        PlayerStress.Instance.AddStress(_stressIncreaseOnTargetDetected);
                        _isChasing = true;
                    }
                    _navMeshAgent.SetDestination(PlayerMovement.Instance.transform.position);
                }
                else
                {
                    _isChasing = false;
                    _navMeshAgent.SetDestination(_patrolPoints[currentPatrolPointIndex] + _initialPosition);
                    if (Vector3.Distance(transform.position, _patrolPoints[currentPatrolPointIndex] + _initialPosition) <= _navMeshAgent.stoppingDistance)
                    {
                        currentPatrolPointIndex = (byte)(currentPatrolPointIndex + indexFactor);
                        if (currentPatrolPointIndex == _patrolPoints.Length - 1) indexFactor = -1;
                        else if (currentPatrolPointIndex == 0 && indexFactor == -1) indexFactor = 1;
                    }
                }
                yield return _delay;
            }
        }

        #region Debug
        private void OnDrawGizmosSelected()
        {
            if (_debugDraw)
            {
                Gizmos.color = _detectionRangeAreaColor;
                Gizmos.DrawMesh(_FOVMesh, transform.position, transform.rotation);

                Gizmos.color = _minDistanceAreaColor;
                Gizmos.DrawSphere(transform.position, _minDistanceFromTarget);

                Gizmos.color = _patrolPointsColor;
                for (int i = 0; i < _patrolPoints.Length; i++)
                {
                    Vector3 pos = _initialPosition != Vector3.zero ? _initialPosition : transform.position;
                    Gizmos.DrawSphere(pos + _patrolPoints[i], _patrolPointRadius);
                }
            }
        }

        private void OnValidate()
        {
            _FOVMesh = CreateConeMesh();
        }

        //creates the visual cone in the editor thar represents the FOV
        private Mesh CreateConeMesh()
        {
            Mesh pyramid = new Mesh();
            int numOfTriangles = 14;
            int numVertices = numOfTriangles * 3;
            float halfAngle = _visionAngle / 2f;
            Vector3[] vertices = new Vector3[numVertices];
            int[] triangles = new int[numVertices];

            Vector3 Center = transform.forward;//0 

            Vector3 Left = Quaternion.Euler(0, -halfAngle, 0) * transform.forward * _detectionRange;
            Vector3 Right = Quaternion.Euler(0, halfAngle, 0) * transform.forward * _detectionRange;

            Vector3 top = Quaternion.Euler(-halfAngle, 0, 0) * transform.forward * _detectionRange;
            Vector3 bottom = Quaternion.Euler(halfAngle, 0, 0) * transform.forward * _detectionRange;

            float middlePointAngles = halfAngle * .7f;

            Vector3 MiddleTopLeft = Quaternion.Euler(-middlePointAngles, -middlePointAngles, 0) * transform.forward * _detectionRange;
            Vector3 MiddleTopRight = Quaternion.Euler(-middlePointAngles, middlePointAngles, 0) * transform.forward * _detectionRange;

            Vector3 MiddleBottomLeft = Quaternion.Euler(middlePointAngles, -middlePointAngles, 0) * transform.forward * _detectionRange;
            Vector3 MiddleBottomRight = Quaternion.Euler(middlePointAngles, middlePointAngles, 0) * transform.forward * _detectionRange;

            int currentVert = 0;

            //top left
            vertices[currentVert++] = top;
            vertices[currentVert++] = Center;
            vertices[currentVert++] = MiddleTopLeft;

            //middle top left
            vertices[currentVert++] = MiddleTopLeft;
            vertices[currentVert++] = Center;
            vertices[currentVert++] = Left;

            //middle bottom left
            vertices[currentVert++] = Left;
            vertices[currentVert++] = Center;
            vertices[currentVert++] = MiddleBottomLeft;

            //bottom left
            vertices[currentVert++] = MiddleBottomLeft;
            vertices[currentVert++] = Center;
            vertices[currentVert++] = bottom;

            //bottom right
            vertices[currentVert++] = bottom;
            vertices[currentVert++] = Center;
            vertices[currentVert++] = MiddleBottomRight;

            //middle bottom right
            vertices[currentVert++] = MiddleBottomRight;
            vertices[currentVert++] = Center;
            vertices[currentVert++] = Right;

            //middle top right
            vertices[currentVert++] = Right;
            vertices[currentVert++] = Center;
            vertices[currentVert++] = MiddleTopRight;

            //top right
            vertices[currentVert++] = MiddleTopRight;
            vertices[currentVert++] = Center;
            vertices[currentVert++] = top;

            //pyramid base
            //top
            vertices[currentVert++] = top;
            vertices[currentVert++] = MiddleTopLeft;
            vertices[currentVert++] = MiddleTopRight;

            //top middle
            vertices[currentVert++] = MiddleTopRight;
            vertices[currentVert++] = MiddleTopLeft;
            vertices[currentVert++] = Right;

            vertices[currentVert++] = Right;
            vertices[currentVert++] = MiddleTopLeft;
            vertices[currentVert++] = Left;

            //bottom middle
            vertices[currentVert++] = Left;
            vertices[currentVert++] = MiddleBottomRight;
            vertices[currentVert++] = Right;

            vertices[currentVert++] = MiddleBottomRight;
            vertices[currentVert++] = Left;
            vertices[currentVert++] = MiddleBottomLeft;

            //bottom
            vertices[currentVert++] = MiddleBottomLeft;
            vertices[currentVert++] = bottom;
            vertices[currentVert++] = MiddleBottomRight;

            for (int i = 0; i < numVertices; i++) triangles[i] = i;
            pyramid.vertices = vertices;
            pyramid.triangles = triangles;
            pyramid.RecalculateNormals();

            return pyramid;
        }
        #endregion
    }
}