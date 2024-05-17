using UnityEngine;
using Cinemachine;
using System.Collections;

namespace Ivayami.Dialogue
{
    public class DialogueCamera : MonoSingleton<DialogueCamera>
    {
        private CinemachineVirtualCamera _dialogueCamera;
        private Camera _gameplayCamera;

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
            _gameplayCamera = Camera.main;

            _gameplayCameraPriority = FindObjectOfType<CinemachineFreeLook>().Priority;

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
            else _dialogueCamera.transform.SetPositionAndRotation(_gameplayCamera.transform.position, _gameplayCamera.transform.rotation);
            _currentPositionCurve = cameraTransitionInfo.PositionCurve;
            _currentRotationCurve = cameraTransitionInfo.RotationCurve;
            _finalPlacement = cameraTransitionInfo.transform;
            _currentDuration = cameraTransitionInfo.Duration;
            CameraPriotitySetup();
            _animationCoroutine = StartCoroutine(BlendAnimationCoroutine());
        }

        private IEnumerator BlendAnimationCoroutine()
        {
            float count = 0;
            Vector3 initialPosition = _dialogueCamera.transform.position;
            Quaternion initialRotation = _dialogueCamera.transform.rotation;

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
            if (DialogueController.Instance.LockInput && !_dialogueSetupEventTriggered)
            {
                DialogueController.Instance.OnDialogueEnd += HandleOnDialogueEnd;
                _dialogueSetupEventTriggered = true;
            }
        }

        private void HandleOnDialogueEnd()
        {
            _dialogueCamera.Priority = -999;
            if (DialogueController.Instance.LockInput && _dialogueSetupEventTriggered) DialogueController.Instance.OnDialogueEnd -= HandleOnDialogueEnd;
            _dialogueSetupEventTriggered = false;
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