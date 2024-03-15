using UnityEngine;

namespace Paranapiacaba.Dialogue
{
    public class CameraTransitionInfo : MonoBehaviour
    {
        [SerializeField] private Transform _cameraFinalPositionAndRotation;
        [Min(0)] public float duration;
        public AnimationCurve positionCurve;
        public AnimationCurve rotationCurve;

        public void StartMovement()
        {
            DialogueCamera.Instance.MoveRotate(this);
        }

        public Transform GetValidTransform()
        {
            return _cameraFinalPositionAndRotation ? _cameraFinalPositionAndRotation : transform;
        }
    }
}