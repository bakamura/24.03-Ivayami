using UnityEngine;
using UnityEngine.Serialization;
using Ivayami.Player;
//using System.Collections;

namespace Ivayami.Dialogue
{
    public class CameraAnimationInfo : MonoBehaviour
    {
        [SerializeField] private bool _snapCameraPositionOnInterrupt = true;
        [SerializeField] private bool _changeCameraFocus;
        [SerializeField] private bool _lookAtPlayer;
        [SerializeField] private bool _followPlayer;
        [SerializeField] private Transform _lookAtTarget;
        [SerializeField] private Transform _followTarget;

        [FormerlySerializedAs("duration"), Min(0)] public float Duration;
        [SerializeField] private bool _hidePlayerModel;
        [FormerlySerializedAs("positionCurve")] public AnimationCurve PositionCurve;
        [FormerlySerializedAs("rotationCurve")] public AnimationCurve RotationCurve;

        //private Coroutine _durationinFocusCoroutine;

        public Transform LookAtTarget { get; private set; }
        public Transform FollowTarget { get; private set; }
        public bool ChangeCameraFocus => _changeCameraFocus;
        public bool FollowPlayer => _followPlayer;
        public bool SnapCameraPositionOnInterrupt => _snapCameraPositionOnInterrupt;

        public void StartMovement()
        {
            if (_hidePlayerModel) PlayerMovement.Instance.UpdateVisualsVisibility(false);
            LookAtTarget = transform;
            FollowTarget = transform;
            if (_changeCameraFocus)
            {
                if (_lookAtPlayer) LookAtTarget = PlayerCamera.Instance.CameraAimPoint;
                else LookAtTarget = _lookAtTarget ? _lookAtTarget : transform;
                if (_followPlayer) FollowTarget = PlayerCamera.Instance.CameraAimRotator;
                else FollowTarget = _followTarget ? _followTarget : transform;
            }
            DialogueCamera.Instance.MoveRotate(this);
        }

        public void ExitDialogueCamera()
        {
            if (_hidePlayerModel) PlayerMovement.Instance.UpdateVisualsVisibility(true);
            DialogueCamera.Instance.ExitDialogeCamera();
        }
    }
}