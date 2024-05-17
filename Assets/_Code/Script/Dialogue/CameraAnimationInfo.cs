using UnityEngine;
using UnityEngine.Serialization;

namespace Ivayami.Dialogue
{
    public class CameraAnimationInfo : MonoBehaviour
    {
        [FormerlySerializedAs("duration"), Min(0)] public float Duration;
        [FormerlySerializedAs("positionCurve")] public AnimationCurve PositionCurve;
        [FormerlySerializedAs("rotationCurve")] public AnimationCurve RotationCurve;

        public void StartMovement()
        {
            DialogueCamera.Instance.MoveRotate(this);
        }

        public void ExitDialogueCamera()
        {
            DialogueCamera.Instance.ExitDialogeCamera();
        }
    }
}