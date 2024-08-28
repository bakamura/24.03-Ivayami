using UnityEngine;
using UnityEngine.Serialization;
using Ivayami.Player;
using System.Collections;

namespace Ivayami.Dialogue
{
    public class CameraAnimationInfo : MonoBehaviour
    {
        [SerializeField] private bool _changeCameraFocus;
        [SerializeField] private bool _lookAtPlayer;
        [SerializeField] private bool _followPlayer;
        [SerializeField] private Transform _lookAtTarget;
        [SerializeField] private Transform _followTarget;

        [FormerlySerializedAs("duration"), Min(0)] public float Duration;
        [SerializeField] private bool _hidePlayerModel;
        [FormerlySerializedAs("positionCurve")] public AnimationCurve PositionCurve;
        [FormerlySerializedAs("rotationCurve")] public AnimationCurve RotationCurve;

        private Coroutine _durationinFocusCoroutine;

        public void StartMovement()
        {
            if (_hidePlayerModel) PlayerMovement.Instance.UpdateVisualsVisibility(false);
            if (_changeCameraFocus)
            {
                if (_lookAtPlayer) PlayerCamera.Instance.FreeLookCam.LookAt = PlayerCamera.Instance.CameraAimPoint;
                else PlayerCamera.Instance.FreeLookCam.LookAt = _lookAtTarget;
                if (_followPlayer) PlayerCamera.Instance.FreeLookCam.Follow = PlayerCamera.Instance.CameraAimPoint;
                else PlayerCamera.Instance.FreeLookCam.Follow = _followTarget;
                StartDurationFocusDelay();
            }
            else DialogueCamera.Instance.MoveRotate(this);            
        }

        public void ExitDialogueCamera()
        {            
            if (_hidePlayerModel) PlayerMovement.Instance.UpdateVisualsVisibility(true);
            DialogueCamera.Instance.ExitDialogeCamera();
        }

        private void StartDurationFocusDelay()
        {
            PlayerCamera.Instance.UpdateCameraControls(false);
            if (_durationinFocusCoroutine != null)
            {
                StopCoroutine(_durationinFocusCoroutine);
                _durationinFocusCoroutine = null;
            }
            _durationinFocusCoroutine = StartCoroutine(DurationInFocusCoroutine());
        }

        private IEnumerator DurationInFocusCoroutine()
        {
            yield return new WaitForSeconds(Duration);
            ExitDialogueCamera();
            PlayerCamera.Instance.FreeLookCam.LookAt = PlayerCamera.Instance.CameraAimPoint;
            PlayerCamera.Instance.FreeLookCam.Follow = PlayerCamera.Instance.CameraAimPoint;
            PlayerCamera.Instance.UpdateCameraControls(true);
            _durationinFocusCoroutine = null;
        }
    }
}