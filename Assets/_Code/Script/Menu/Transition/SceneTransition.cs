
namespace Ivayami.UI {
    public class SceneTransition : MonoSingleton<SceneTransition> {

        private MenuGroup _menuGroup;
        public Menu Menu { get; private set; }

        private void Start() {
            _menuGroup = GetComponent<MenuGroup>();
            Menu = GetComponent<Menu>();
            _menuGroup.CloseCurrentThenOpen(Menu);
        }

        public void Transition() {
            Logger.Log(LogType.UI, $"Scene Transition Fade");
            _menuGroup.CloseCurrentThenOpen(Menu);
        }

    }
}
