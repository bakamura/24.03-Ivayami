using UnityEngine;
using Ivayami.Player;

namespace Ivayami.Save
{
    public abstract class SaveObject : MonoBehaviour
    {
        [SerializeField, ReadOnly] private string _id;
        [SerializeField] private bool _saveOnDisable = true;
        private static bool _canSave = true;
        private static bool _eventSubscribed;
        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        protected virtual void Start()
        {
            LoadData();
            if (!_eventSubscribed && PlayerStress.Instance)
            {
                PlayerStress.Instance.onFail.AddListener(HandlePlayerDeath);
                _eventSubscribed = true;
            }
        }

        public abstract void SaveData();

        public abstract void LoadData();

        protected virtual void OnEnable()
        {
            if (SaveSystem.Instance)
            {
                SaveSystem.Instance.RegisterSaveObject(this);
                //PlayerStress.Instance.onFail.AddListener(HandlePlayerDeath);
            }
        }

        protected virtual void OnDisable()
        {
            if (SaveSystem.Instance)
            {
                if (_saveOnDisable && _canSave) SaveData();
                SaveSystem.Instance.UnregisterSaveObject(this);
                //PlayerStress.Instance.onFail.RemoveListener(HandlePlayerDeath);
                //_canSave = true;
            }
        }

        private void OnDestroy()
        {
            _canSave = true;
        }

        private static void HandlePlayerDeath()
        {
            if (!PlayerStress.Instance.OverrideFailLoadValue) _canSave = false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ID)) ID = gameObject.name + "_" + Mathf.Abs(this.GetInstanceID());
        }
#endif
    }
}