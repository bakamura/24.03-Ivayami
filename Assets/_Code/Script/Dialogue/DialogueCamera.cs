using UnityEngine;
using Cinemachine;
using System.Collections;
using Ivayami.Player;
using Ivayami.Audio;

namespace Ivayami.Dialogue
{
    public class DialogueCamera : MonoSingleton<DialogueCamera>
    {
        private CinemachineVirtualCamera _dialogueCamera;

        private int _gameplayCameraPriority;
        private Coroutine _animationCoroutine;
        private AnimationCurve _currentPositionCurve;
        private AnimationCurve _currentRotationCurve;
        private Transform _currentLookAt;
        private Transform _currentFollow;
        private bool _currentChangeTargetFocus;
        private bool _currentFollowPlayer;
        private bool _dialogueSetupEventTriggered;
        private float _currentDuration;

        private void Start()
        {
            _dialogueCamera = GetComponentInChildren<CinemachineVirtualCamera>();

            _gameplayCameraPriority = PlayerCamera.Instance.FreeLookCam.Priority;

            DialogueController.Instance.OnSkipSpeech += HandleOnSkipSpeech;
        }

        public void MoveRotate(CameraAnimationInfo cameraTransitionInfo)
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                if (_currentChangeTargetFocus) PlayerCamera.Instance.UpdateCameraControls(true);
                _animationCoroutine = null;
                TeleportCameraToPositionAndRotation();
            }
            else _dialogueCamera.transform.SetPositionAndRotation(PlayerCamera.Instance.MainCamera.transform.position, PlayerCamera.Instance.MainCamera.transform.rotation);
            _currentChangeTargetFocus = cameraTransitionInfo.ChangeCameraFocus;
            _currentFollowPlayer = cameraTransitionInfo.FollowPlayer;
            _currentPositionCurve = cameraTransitionInfo.PositionCurve;
            _currentRotationCurve = cameraTransitionInfo.RotationCurve;
            _currentFollow = cameraTransitionInfo.FollowTarget;
            _currentLookAt = cameraTransitionInfo.LookAtTarget;
            _currentDuration = cameraTransitionInfo.Duration;
            if (_currentChangeTargetFocus) PlayerCamera.Instance.UpdateCameraControls(false);
            CameraPriotitySetup();
            if (DialogueController.Instance.CurrentDialogue) PlayerAudioListener.Instance.UpdateAudioSource(false);
            _animationCoroutine = StartCoroutine(BlendAnimationCoroutine());
        }

        private void TeleportCameraToPositionAndRotation()
        {
            Vector3 offset = _currentChangeTargetFocus && _currentFollowPlayer ?
                Vector3.Distance(_dialogueCamera.transform.position, _currentFollow.transform.position) * -_dialogueCamera.transform.forward : Vector3.zero;
            _dialogueCamera.transform.SetPositionAndRotation(_currentFollow.position + offset, _currentChangeTargetFocus ? Quaternion.LookRotation(
                    (_currentLookAt.transform.position - _dialogueCamera.transform.position).normalized) : _currentLookAt.rotation);
        }

        private IEnumerator BlendAnimationCoroutine()
        {
            float count = 0;
            _dialogueCamera.transform.GetPositionAndRotation(out Vector3 initialPosition, out Quaternion initialRotation);
            Vector3 offset = _currentChangeTargetFocus && _currentFollowPlayer ?
                Vector3.Distance(_dialogueCamera.transform.position, _currentFollow.transform.position) * -_dialogueCamera.transform.forward : Vector3.zero;
            if (_currentDuration == 0)
            {
                TeleportCameraToPositionAndRotation();
            }
            else
            {
                while (count < 1)
                {
                    _dialogueCamera.transform.SetPositionAndRotation(
                        Vector3.Lerp(initialPosition, _currentFollow.position + offset, _currentPositionCurve.Evaluate(count)),
                        Quaternion.Lerp(initialRotation, _currentChangeTargetFocus ?
                        Quaternion.LookRotation((_currentLookAt.transform.position - initialPosition).normalized) : _currentLookAt.rotation, _currentRotationCurve.Evaluate(count)));
                    count += Time.deltaTime / _currentDuration;
                    yield return null;
                }
            }
            _animationCoroutine = null;
            if (_currentChangeTargetFocus)
            {
                PlayerCamera.Instance.UpdateCameraControls(true);
                _currentChangeTargetFocus = false;
            }
        }

        private void CameraPriotitySetup()
        {
            _dialogueCamera.Priority = _gameplayCameraPriority + 1;
            if (DialogueController.Instance.CurrentDialogue && !_dialogueSetupEventTriggered)
            {
                DialogueController.Instance.OnDialogueEnd += HandleOnDialogueEnd;
                _dialogueSetupEventTriggered = true;
            }
        }

        private void HandleOnDialogueEnd()
        {
            _dialogueCamera.Priority = -999;
            if (_dialogueSetupEventTriggered) DialogueController.Instance.OnDialogueEnd -= HandleOnDialogueEnd;
            _dialogueSetupEventTriggered = false;
            if (_currentChangeTargetFocus)
            {
                PlayerCamera.Instance.UpdateCameraControls(true);
                _currentChangeTargetFocus = false;
            }
            PlayerAudioListener.Instance.UpdateAudioSource(true);
        }

        public void ExitDialogeCamera()
        {
            HandleOnDialogueEnd();
        }

        private void HandleOnSkipSpeech()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
                TeleportCameraToPositionAndRotation();
            }
        }

    }
}