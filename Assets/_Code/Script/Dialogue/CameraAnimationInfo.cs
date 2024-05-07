using UnityEngine;

namespace Ivayami.Dialogue
{
    public class CameraAnimationInfo : MonoBehaviour
    {
        [Min(0)] public float duration;
        public AnimationCurve positionCurve;
        public AnimationCurve rotationCurve;

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