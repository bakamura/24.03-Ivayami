using UnityEngine;

namespace Ivayami.Save
{
    public abstract class SaveObject : MonoBehaviour
    {
        [SerializeField, ReadOnly] private string _id;        
        private static bool _canSave = true;
        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        protected virtual void Start()
        {
            LoadData();
        }

        public abstract void SaveData();

        public abstract void LoadData();

        protected virtual void OnEnable()
        {
            if (SaveSystem.Instance)
            {
                SaveSystem.Instance.RegisterSaveObject(this);
            }
        }

        protected virtual void OnDisable()
        {
            if (SaveSystem.Instance)
            {
                if (_canSave) SaveData();
                SaveSystem.Instance.UnregisterSaveObject(this);
            }
        }

        public static void UpdateSaveLock(bool canSave)
        {
            _canSave = canSave;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ID)) ID = gameObject.name + "_"+ UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name + "_" + Mathf.Abs(this.GetInstanceID());
        }
#endif
    }
}