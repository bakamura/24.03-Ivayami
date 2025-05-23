using System.Collections;
using UnityEngine;

namespace Ivayami.Enemy
{
    public class EnemySoundDetector : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _detectionRange;
        [SerializeField, Min(.02f)] private float _checkSoundTickFrequency = .5f;
        [SerializeField] private LayerMask _blockLayers;
#if UNITY_EDITOR
        [SerializeField] private bool _drawGizmos;
        [SerializeField] private Color _gizmoColor = Color.black;
        private EnemySoundPoints.SoundPointData _currentData;
#endif
        private Coroutine _checkSoundCoroutine;
        private ISoundDetection _target;

        private void Awake()
        {
            if (!EnemySoundPoints.Instance) return;
            _target = GetComponentInParent<ISoundDetection>();
            if (_target == null) Debug.LogError("No Sound Point User found in hierarchy");
        }

        private void OnEnable()
        {
            if (!EnemySoundPoints.Instance) return;
            if (_checkSoundCoroutine == null) _checkSoundCoroutine = StartCoroutine(CheckForSoundsCoroutine());
        }

        private void OnDisable()
        {
            if (!EnemySoundPoints.Instance) return;
            if(_checkSoundCoroutine != null)
            {
                StopCoroutine(_checkSoundCoroutine);
                _checkSoundCoroutine = null;
            }
        }

        private IEnumerator CheckForSoundsCoroutine()
        {
            WaitForSeconds delay = new WaitForSeconds(_checkSoundTickFrequency);
            EnemySoundPoints.SoundPointData data;
            while (true)
            {
                data = EnemySoundPoints.Instance.GetClosestPointToSoundPoint(transform.position, _detectionRange);
                if(data.IsValid()) _target.GoToSoundPosition(data);
#if UNITY_EDITOR
                _currentData = data;
#endif
                yield return delay;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_drawGizmos) return;
            Gizmos.color = _gizmoColor;
            Gizmos.DrawSphere(transform.position, _detectionRange);

            if (_currentData.IsValid())
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;
            if (_currentData.IsValid())
            {
                Gizmos.DrawLine(transform.position, _currentData.Position);
                Gizmos.DrawSphere(_currentData.Position, _currentData.Radius);
            }
        }
#endif
    }
}