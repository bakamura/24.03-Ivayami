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
            //if (StopAnimationCoroutine())
            //{
            //    //if (_currentChangeTargetFocus) PlayerCamera.Instance.UpdateCameraControls(true);
            //}
            //else _dialogueCamera.transform.SetPositionAndRotation(PlayerCamera.Instance.MainCamera.transform.position, PlayerCamera.Instance.MainCamera.transform.rotation);
            if (!StopAnimationCoroutine())
                _dialogueCamera.transform.SetPositionAndRotation(PlayerCamera.Instance.MainCamera.transform.position, PlayerCamera.Instance.MainCamera.transform.rotation);

            _currentChangeTargetFocus = cameraTransitionInfo.ChangeCameraFocus;
            _currentFollowPlayer = cameraTransitionInfo.FollowPlayer;
            _currentPositionCurve = cameraTransitionInfo.PositionCurve;
            _currentRotationCurve = cameraTransitionInfo.RotationCurve;
            _currentFollow = cameraTransitionInfo.FollowTarget;
            _currentLookAt = cameraTransitionInfo.LookAtTarget;
            _currentDuration = cameraTransitionInfo.Duration;
            //if (_currentChangeTargetFocus) PlayerCamera.Instance.UpdateCameraControls(false);
            CameraPriotitySetup();
            if (DialogueController.Instance.CurrentDialogue) PlayerAudioListener.Instance.UpdateAudioSource(false);
            _animationCoroutine = StartCoroutine(BlendAnimationCoroutine());
        }

        private void TeleportCameraToPositionAndRotation()
        {
            Vector3 offset = _currentChangeTargetFocus && _currentFollowPlayer ?
                Vector3.Distance(_dialogueCamera.transform.position, _currentFollow.position) * (_currentFollow.position - _currentLookAt.position).normalized
                + PlayerCamera.Instance.CameraAimPoint.localPosition.x * _dialogueCamera.transform.right : Vector3.zero;
            _dialogueCamera.transform.SetPositionAndRotation(_currentFollow.position + offset, _currentChangeTargetFocus ? Quaternion.LookRotation(
                    (_currentLookAt.transform.position - _dialogueCamera.transform.position).normalized) : _currentLookAt.rotation);
        }

        private IEnumerator BlendAnimationCoroutine()
        {
            _dialogueCamera.transform.GetPositionAndRotation(out Vector3 initialPosition, out Quaternion initialRotation);
            if (_currentDuration == 0)
            {
                TeleportCameraToPositionAndRotation();
            }
            else
            {
                float count = 0;
                Vector3 offset = Vector3.zero;
                float distance = Vector3.Distance(_dialogueCamera.transform.position, _currentFollow.position);
                float xDistance = PlayerCamera.Instance.CameraAimPoint.localPosition.x;
                while (count < 1)
                {
                    if (_currentChangeTargetFocus && _currentFollowPlayer)
                        offset = distance * (_currentFollow.position - _currentLookAt.position).normalized + xDistance * _dialogueCamera.transform.right;

                    _dialogueCamera.transform.SetPositionAndRotation(
                        Vector3.Lerp(initialPosition, _currentFollow.position + offset, _currentPositionCurve.Evaluate(count)),
                        Quaternion.Lerp(initialRotation, _currentChangeTargetFocus ?
                        Quaternion.LookRotation((_currentLookAt.transform.position - _dialogueCamera.transform.position).normalized) : _currentLookAt.rotation, _currentRotationCurve.Evaluate(count)));
                    count += Time.deltaTime / _currentDuration;
                    yield return null;
                }
            }
            RecalculateCameraOrientation();
            _animationCoroutine = null;
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
            if (StopAnimationCoroutine())
            {
                RecalculateCameraOrientation();
            }
            PlayerAudioListener.Instance.UpdateAudioSource(true);
        }

        public void ExitDialogeCamera()
        {
            HandleOnDialogueEnd();
        }

        private bool StopAnimationCoroutine()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
                TeleportCameraToPositionAndRotation();
                return true;
            }
            return false;
        }

        private void HandleOnSkipSpeech()
        {
            StopAnimationCoroutine();
        }

        private void RecalculateCameraOrientation()
        {
            if (_currentChangeTargetFocus)
            {
                PlayerCamera.Instance.FreeLookCam.PreviousStateIsValid = false;
                PlayerCamera.Instance.FreeLookCam.m_XAxis.Value = _dialogueCamera.transform.rotation.eulerAngles.y;
                PlayerCamera.Instance.FreeLookCam.m_YAxis.Value = .5f;
                //PlayerCamera.Instance.UpdateCameraControls(true);
                _currentChangeTargetFocus = false;
            }
        }
    }
}