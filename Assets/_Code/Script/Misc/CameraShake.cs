using System.Collections;
using UnityEngine;
using Cinemachine;
using Ivayami.Player;
using Ivayami.Dialogue;

namespace Ivayami.Misc
{
    public sealed class CameraShake : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("If 0 will need to call StopScreenShake manually")] private float _shakeDuration;
        [SerializeField, Min(0f)] private float _shakeAmplitude;
        [SerializeField] private AnimationCurve _shakeAmplitudeCurve;
        [SerializeField, Min(0f)] private float _shakeFrequency;
        [SerializeField] private AnimationCurve _shakeFrequencyCurve;

        private static Coroutine _shakeCoroutine;
        private CinemachineBasicMultiChannelPerlin[] _currentCinemachinePerlin;
        private bool _isPlaying;
        private bool _wasInDialogue;

        private void OnDisable()
        {
            StopScreenShake();
        }

        public void StartScreenShake()
        {
            StartCoroutine(StartScreenShakeCoroutine());
        }

        private IEnumerator StartScreenShakeCoroutine()
        {
            yield return new WaitForEndOfFrame();
            _currentCinemachinePerlin = PlayerCamera.Instance.CinemachineBrain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponentsInChildren<CinemachineBasicMultiChannelPerlin>();
            EndScreenShake();
            if (DialogueController.Instance.CurrentDialogue)
            {
                DialogueController.Instance.OnDialogueEnd += StopScreenShake;
                _wasInDialogue = true;
            }
            _isPlaying = true;
            if (_shakeDuration > 0f) _shakeCoroutine = StartCoroutine(ShakeCoroutine());
            else
            {
                for (int i = 0; i < _currentCinemachinePerlin.Length; i++)
                {
                    _currentCinemachinePerlin[i].m_FrequencyGain = _shakeFrequency;
                    _currentCinemachinePerlin[i].m_AmplitudeGain = _shakeAmplitude;
                }
            }
        }

        public void StopScreenShake()
        {
            if (_isPlaying)
            {
                //Debug.Log($"Camera Shake Interrupted By {name}");
                EndScreenShake();
            }
        }

        private IEnumerator ShakeCoroutine()
        {
            float count = 0;
            while (count < 1f)
            {
                count += Time.deltaTime / _shakeDuration;
                for (int i = 0; i < _currentCinemachinePerlin.Length; i++)
                {
                    _currentCinemachinePerlin[i].m_FrequencyGain = Mathf.Lerp(0, _shakeFrequency, _shakeFrequencyCurve.Evaluate(count));
                    _currentCinemachinePerlin[i].m_AmplitudeGain = Mathf.Lerp(0, _shakeAmplitude, _shakeAmplitudeCurve.Evaluate(count));
                }
                yield return null;
            }
            EndScreenShake();
        }

        private void EndScreenShake()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }
            for (int i = 0; i < _currentCinemachinePerlin.Length; i++)
            {
                _currentCinemachinePerlin[i].m_FrequencyGain = 0;
                _currentCinemachinePerlin[i].m_AmplitudeGain = 0;
            }
            if (_wasInDialogue)
            {
                DialogueController.Instance.OnDialogueEnd -= StopScreenShake;
                _wasInDialogue = false;
            }
            _isPlaying = false;
        }
    }
}
