using UnityEngine;

namespace Ivayami.UI {
    public class CompositeMenu : Menu {

        [Header("Composite Menu")]

        [SerializeField] private Menu[] _menus;

        protected override void Awake() {
            base.Awake();

            _menus = GetComponents<Menu>();
        }

        public override void Open() {
            foreach (Menu menu in _menus) menu.Open();
        }

        public override void Close() {
            foreach (Menu menu in _menus) menu.Close();
        }

    }
}