using UnityEngine;

namespace Paranapiacaba.Dialogue
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
    }
}