using System.Collections;
using UnityEngine;

namespace Ivayami.Enemy
{
    public class EnemySoundArea : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _radius;
        [SerializeField, Min(.02f)] private float _sendSoundRepeatedlyInterval = .5f;
        [SerializeField, Min(.02f)] private float _soundDuration = 2f;
        [SerializeField, Tooltip("If true will generate a sound area when the gameobject activates and remove area on deactivate")] private bool _generateSoundOnEnable;
#if UNITY_EDITOR
        [SerializeField] private Color _debugColor = Color.yellow;
        [SerializeField] private bool _gizmoAlwaysOn;
#endif

        private Coroutine _soundDurationCoroutine;
        private Coroutine _repeatSoundCoroutine;
        private float _currentSoundDuration;
        private void OnEnable()
        {
            if (!SoundPoints.Instance) return;
            if (!_generateSoundOnEnable) return;
            UpdateSoundPoint();
        }

        private void OnDisable()
        {
            if (!SoundPoints.Instance) return;
            StopGenerateSoundRepeatedly();
            if (_soundDurationCoroutine != null)
            {
                StopCoroutine(_soundDurationCoroutine);
                _soundDurationCoroutine = null;
            }
            RemoveSound();
        }

        private void UpdateSoundPoint()
        {
            SoundPoints.Instance.UpdateSoundPoint(nameof(EnemySoundArea) + gameObject.name + GetInstanceID(), new SoundPoints.SoundPointData(transform.position, _radius));
        }

        public void GenerateSound()
        {
            UpdateSoundPoint();
            _currentSoundDuration = _soundDuration;
            if (_soundDurationCoroutine != null)
            {
                _soundDurationCoroutine = StartCoroutine(SoundDurationCoroutine());
            }
        }

        public void GenerateSoundRepeatedly()
        {
            if (_repeatSoundCoroutine != null)
            {
                _repeatSoundCoroutine = StartCoroutine(RepeatSoundCoroutine());
            }
        }

        public void StopGenerateSoundRepeatedly()
        {
            if (_repeatSoundCoroutine != null)
            {
                StopCoroutine(_repeatSoundCoroutine);
                _repeatSoundCoroutine = null;
                RemoveSound();
            }
        }

        private void RemoveSound()
        {
            SoundPoints.Instance.RemoveSoundPoint(nameof(EnemySoundArea) + gameObject.name + GetInstanceID());
        }

        private IEnumerator RepeatSoundCoroutine()
        {
            WaitForSeconds delay = new WaitForSeconds(_sendSoundRepeatedlyInterval);
            while (true)
            {
                UpdateSoundPoint();
                yield return delay;
            }
        }

        private IEnumerator SoundDurationCoroutine()
        {
            while (_currentSoundDuration > 0)
            {
                _currentSoundDuration -= Time.deltaTime;
                yield return null;
            }
            RemoveSound();
            _soundDurationCoroutine = null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_gizmoAlwaysOn)
            {
                DrawGizmos();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_gizmoAlwaysOn) DrawGizmos();
        }

        private void DrawGizmos()
        {
            Gizmos.color = _debugColor;
            Gizmos.DrawSphere(transform.position, _radius);
        }
#endif
    }
}