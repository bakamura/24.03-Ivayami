using UnityEngine;

namespace Ivayami.Scene
{
    public class ForceUnloadSceneAssets : MonoBehaviour
    {
        private void Start()
        {
            SceneController.Instance.OnStartUnloadScene += HandleOnStartUnloadScene;
        }

        private void DestroyAll()
        {
            SceneController.Instance.UpdateSceneControllerActiveState(false);
            SceneController.Instance.OnStartUnloadScene -= HandleOnStartUnloadScene;
            Destroy(gameObject);
            AsyncOperation operation = Resources.UnloadUnusedAssets();
            operation.completed += (operation) => SceneController.Instance.UpdateSceneControllerActiveState(true);
        }

        private void HandleOnStartUnloadScene(string sceneId)
        {
            if (string.Equals(gameObject.scene.name, sceneId)) DestroyAll();
        }
    }
}