using UnityEngine;
using UnityEngine.Serialization;
using Ivayami.Player;

namespace Ivayami.Dialogue
{
    public class CameraAnimationInfo : MonoBehaviour
    {
        [FormerlySerializedAs("duration"), Min(0)] public float Duration;
        [SerializeField] private bool _hidePlayerModel;
        [FormerlySerializedAs("positionCurve")] public AnimationCurve PositionCurve;
        [FormerlySerializedAs("rotationCurve")] public AnimationCurve RotationCurve;

        public void StartMovement()
        {
            if(_hidePlayerModel) PlayerMovement.Instance.UpdateVisualsVisibility(false);
            DialogueCamera.Instance.MoveRotate(this);
        }

        public void ExitDialogueCamera()
        {
            if (_hidePlayerModel) PlayerMovement.Instance.UpdateVisualsVisibility(true);
            DialogueCamera.Instance.ExitDialogeCamera();
        }
    }
}