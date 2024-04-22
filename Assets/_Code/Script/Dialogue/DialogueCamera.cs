using UnityEngine;
using UnityEngine.Serialization;
using Cinemachine;
using System.Collections;

namespace Ivayami.Dialogue {
    public class DialogueCamera : MonoSingleton<DialogueCamera> {

        //[SerializeField] private float _defaultDuration = 1f;
        //[SerializeField, FormerlySerializedAs("Default Position Blend")] private AnimationCurve _defaultPositionCurve;
        //[SerializeField, FormerlySerializedAs("Default Rotation Blend")] private AnimationCurve _defaultRotationCurve;
        private CinemachineVirtualCamera _dialogueCamera;
        private Camera _gameplayCamera;

        private int _gameplayCameraPriority;
        private Coroutine _animationCoroutine;
        private AnimationCurve _currentPositionCurve;
        private AnimationCurve _currentRotationCurve;
        private Transform _finalPlacement;
        private float _currentDuration;

        private void Start()
        {
            _dialogueCamera = GetComponentInChildren<CinemachineVirtualCamera>();
            _gameplayCamera = Camera.main;

            _gameplayCameraPriority = FindObjectOfType<CinemachineFreeLook>().Priority;

            DialogueController.Instance.OnDialogeStart += HandleOnDialogeStart;
            DialogueController.Instance.OnDialogueEnd += HandleOnDialogueEnd;
            DialogueController.Instance.OnSkipSpeech += HandleOnSkipSpeech;
        }

        //public void MoveRotate(Transform target) {
        //    if(_animationCoroutine == null && target)
        //    {
        //        _currentPositionCurve = _defaultPositionCurve;
        //        _currentRotationCurve = _defaultRotationCurve;
        //        _finalPlacement = target;
        //        _currentDuration = _defaultDuration;
        //        _animationCoroutine = StartCoroutine(BlendAnimationCoroutine());
        //    }
        //}

        public void MoveRotate(CameraAnimationInfo cameraTransitionInfo, bool willBeInDialogue)
        {
            if (_animationCoroutine == null && cameraTransitionInfo)
            {
                _currentPositionCurve = cameraTransitionInfo.positionCurve;
                _currentRotationCurve = cameraTransitionInfo.rotationCurve;
                _finalPlacement = cameraTransitionInfo.transform;
                _currentDuration = cameraTransitionInfo.duration;
                if (!willBeInDialogue) HandleOnDialogeStart();
                _animationCoroutine = StartCoroutine(BlendAnimationCoroutine());
            }
        }

        private IEnumerator BlendAnimationCoroutine()
        {
            float count = 0;
            Vector3 initialPosition = _dialogueCamera.transform.position;
            Quaternion initialRotation = _dialogueCamera.transform.rotation;

            if(_currentDuration == 0)
            {
                _dialogueCamera.transform.SetPositionAndRotation(_finalPlacement.position, _finalPlacement.rotation);
            }
            else
            {
                while(count < 1)
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

        private void HandleOnDialogeStart()
        {
            _dialogueCamera.transform.SetPositionAndRotation(_gameplayCamera.transform.position, _gameplayCamera.transform.rotation);
            _dialogueCamera.Priority = _gameplayCameraPriority + 1;
        }  
        
        private void HandleOnDialogueEnd()
        {
            _dialogueCamera.Priority = -999;
        }

        public void ExitDialogeCamera()
        {
            HandleOnDialogueEnd();
        }

        private void HandleOnSkipSpeech()
        {
            if(_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
                _dialogueCamera.transform.SetPositionAndRotation(_finalPlacement.position, _finalPlacement.rotation);
            }
        }

    }
}