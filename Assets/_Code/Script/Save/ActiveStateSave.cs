using UnityEngine;
using System;

namespace Ivayami.Save
{
    public sealed class ActiveStateSave : SaveObject
    {
        [SerializeField] private GameObject[] _objectsToSave;

        [Serializable]
        public class Data
        {
            public bool[] ObjectStates;

            public Data(Data data = null)
            {
                ObjectStates = data?.ObjectStates;
            }
        }

        private Data _currentData = new Data();
        private Data _newData = new Data();

        protected override void OnEnable()
        {
            base.OnEnable();
            _currentData.ObjectStates = new bool[_objectsToSave.Length];
            for (int i = 0; i < _objectsToSave.Length; i++) _currentData.ObjectStates[i] = _objectsToSave[i].activeSelf;
        }

        public override void SaveData()
        {
            if (!SaveSystem.Instance && SaveSystem.Instance.Progress == null) return;
            _newData.ObjectStates = new bool[_objectsToSave.Length];
            for (int i = 0; i < _objectsToSave.Length; i++) _newData.ObjectStates[i] = _objectsToSave[i].activeSelf;
            if (!IsSameValue(_newData.ObjectStates, _currentData.ObjectStates))
            {
                _currentData = new Data(_newData);
                SaveSystem.Instance.Progress.RecordSaveObject(ID, _currentData);
            }
            //else Debug.Log($"Wont save {name} because its not needed");
        }

        public override void LoadData()
        {
            if (!SaveSystem.Instance || SaveSystem.Instance.Progress == null) return;
            if (SaveSystem.Instance.Progress.GetSaveObjectOfType<Data>(ID, out Data data))
            {
                _currentData = data;
                for (int i = 0; i < _currentData.ObjectStates.Length; i++)
                {
                    _objectsToSave[i].SetActive(_currentData.ObjectStates[i]);
                }
            }
        }

        private bool IsSameValue(bool[] first, bool[] second)
        {
            //if (first.Length != second.Length) return false;
            for(int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i]) return false;
            }
            return true;
        }
    }
}