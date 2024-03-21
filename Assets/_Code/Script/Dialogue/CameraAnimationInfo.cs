using UnityEngine;

namespace Paranapiacaba.Dialogue
{
    public class CameraAnimationInfo : MonoBehaviour
    {
        [Min(0)] public float duration;
        public AnimationCurve positionCurve;
        public AnimationCurve rotationCurve;
        [SerializeField] private bool _willBeInDialogue = true;

        public void StartMovement()
        {
            DialogueCamera.Instance.MoveRotate(this, _willBeInDialogue);
        }

        public void ExitDialogueCamera()
        {
            DialogueCamera.Instance.ExitDialogeCamera();
        }
    }
}