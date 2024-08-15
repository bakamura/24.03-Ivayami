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
        }

        private Data _data = new Data();

        public override void SaveData()
        {
            _data.ObjectStates = new bool[_objectsToSave.Length];
            for (int i = 0; i < _objectsToSave.Length; i++) _data.ObjectStates[i] = _objectsToSave[i].activeSelf;
            SaveSystem.Instance.Progress.RecordSaveObject(ID, _data);
        }

        public override void LoadData()
        {
            if(SaveSystem.Instance.Progress.GetSaveObjectOfType<Data>(ID, out Data data))
            {
                _data = data;
                for (int i = 0; i < _data.ObjectStates.Length; i++)
                {
                    _objectsToSave[i].SetActive(_data.ObjectStates[i]);
                }
            }
        }
    }
}