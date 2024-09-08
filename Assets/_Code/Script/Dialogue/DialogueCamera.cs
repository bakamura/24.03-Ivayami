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
        private Transform _finalPlacement;
        private float _currentDuration;
        private bool _dialogueSetupEventTriggered;

        private void Start()
        {
            _dialogueCamera = GetComponentInChildren<CinemachineVirtualCamera>();

            _gameplayCameraPriority = PlayerCamera.Instance.FreeLookCam.Priority;

            //DialogueController.Instance.OnDialogeStart += HandleOnDialogeStart;           
            DialogueController.Instance.OnSkipSpeech += HandleOnSkipSpeech;
        }

        public void MoveRotate(CameraAnimationInfo cameraTransitionInfo)
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
                _dialogueCamera.transform.SetPositionAndRotation(_finalPlacement.position, _finalPlacement.rotation);
            }
            else _dialogueCamera.transform.SetPositionAndRotation(PlayerCamera.Instance.MainCamera.transform.position, PlayerCamera.Instance.MainCamera.transform.rotation);
            _currentPositionCurve = cameraTransitionInfo.PositionCurve;
            _currentRotationCurve = cameraTransitionInfo.RotationCurve;
            _finalPlacement = cameraTransitionInfo.transform;
            _currentDuration = cameraTransitionInfo.Duration;
            CameraPriotitySetup();
            if (DialogueController.Instance.CurrentDialogue) PlayerAudioListener.Instance.UpdateAudioSource(false);
            _animationCoroutine = StartCoroutine(BlendAnimationCoroutine());
        }

        private IEnumerator BlendAnimationCoroutine()
        {
            float count = 0;
            _dialogueCamera.transform.GetPositionAndRotation(out Vector3 initialPosition, out Quaternion initialRotation);
            if (_currentDuration == 0)
            {
                _dialogueCamera.transform.SetPositionAndRotation(_finalPlacement.position, _finalPlacement.rotation);
            }
            else
            {
                while (count < 1)
                {
                    _dialogueCamera.transform.SetPositionAndRotation(
                        Vector3.Lerp(initialPosition, _finalPlacement.position, _currentPositionCurve.Evaluate(count)),
                        Quaternion.Lerp(initialRotation, _finalPlacement.rotation, _currentRotationCurve.Evaluate(count)));
                    count += Time.deltaTime / _currentDuration;
                    yield return null;
                }
            }
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
                _dialogueCamera.transform.SetPositionAndRotation(_finalPlacement.position, _finalPlacement.rotation);
            }
        }

    }
}