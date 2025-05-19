using UnityEngine;
using UnityEngine.UI;
using Ivayami.Save;

namespace Ivayami.UI
{
    [RequireComponent(typeof(Fade))]
    public class DeleteSave : MonoBehaviour
    {
        [SerializeField] private MenuGroup _menuGroup;
        [SerializeField] private Selectable _cancelBtn;
        [SerializeField] private Selectable[] _deleteSaveBtns;

        private Fade _fade;
        private byte _currentSaveId;

        private void Awake()
        {
            _fade = GetComponent<Fade>();
            _fade.OnOpenStart.AddListener(UpdateButtons);
        }

        public void SetCurrentSelectedSave(int id)
        {
            _currentSaveId = (byte)id;
        }

        public void DeleteSaveSelected()
        {
            SaveSelector.Instance.UpdateSaveBtnInfo(_currentSaveId);
            SaveSystem.Instance.DeleteProgress(_currentSaveId);
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            GameObject selected = null;
            for(byte i = 0; i < _deleteSaveBtns.Length; i++)
            {
                _deleteSaveBtns[i].interactable = SaveSelector.Instance.CheckForSaveInSlot(i);
                if (_deleteSaveBtns[i].interactable && !selected) selected = _deleteSaveBtns[i].gameObject;
            }
            if (!selected) selected = _cancelBtn.gameObject;
            _menuGroup.SetSelected(selected);
        }
    }
}