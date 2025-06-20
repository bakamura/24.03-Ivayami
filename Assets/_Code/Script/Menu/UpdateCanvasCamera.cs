using UnityEngine;
using Ivayami.Player;

namespace Ivayami.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UpdateCanvasCamera : MonoBehaviour
    {
        private void Start()
        {
            if (!PlayerCamera.Instance) return;
            Canvas canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = PlayerCamera.Instance.UICamera;
            canvas.planeDistance = 1f;
        }
    }
}