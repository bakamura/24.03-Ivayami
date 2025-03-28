namespace Ivayami.Scene
{
    public class SceneLoadersManager : MonoSingleton<SceneLoadersManager>
    {
        private void Start()
        {
            SceneController.Instance.OnLoadScene += HandleOnLoadScene;
            SceneController.Instance.OnStartUnloadScene += HandleOnStartUnloadScene;
            gameObject.SetActive(false);
        }

        private void HandleOnStartUnloadScene(string obj)
        {
            if (string.Equals("MainMenu", obj)) gameObject.SetActive(true);
        }

        private void HandleOnLoadScene(string obj)
        {
            if (string.Equals("MainMenu", obj)) gameObject.SetActive(false);
        }
    }
}