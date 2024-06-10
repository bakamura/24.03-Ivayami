using UnityEngine;
using UnityEngine.UI;

namespace Ivayami.UI {
    public class ReturnActionBtn : MonoBehaviour {

        public void SubscribeButton(Button btn) {
            btn.onClick.AddListener(RemoveReturnAction); // May cause problems because can be assigned infinitely
            ReturnAction.Instance.Set(() => btn.onClick.Invoke());

            Logger.Log(LogType.UI, $"{btn.name} is now the Back Button");
        }

        private void RemoveReturnAction() {
            ReturnAction.Instance.Set(null);

            Logger.Log(LogType.UI, $"Back Button is now none");
        }

    }
}
