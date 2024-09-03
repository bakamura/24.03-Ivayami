using UnityEngine;
using Ivayami.Player;
using Ivayami.Save;
using Ivayami.UI;

namespace Ivayami.Scene {
    public class EnterGameHelper : MonoBehaviour {

        private void Awake() {
            SceneController.Instance.OnAllSceneRequestEnd += EnablePlayerInput;
        }

        private void EnablePlayerInput() {
            PlayerMovement.Instance.SetPosition(SavePoint.Points[SaveSystem.Instance.Progress.pointId].spawnPoint.position);
            
            SceneController.Instance.OnAllSceneRequestEnd -= EnablePlayerInput;
            
            Logger.Log(LogType.UI, $"EnablePlayerInput callback");
        }

    }
}
