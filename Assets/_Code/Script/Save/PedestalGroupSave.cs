using System.Collections.Generic;
using UnityEngine;
using Ivayami.Puzzle;
using System;

namespace Ivayami.Save
{
    [RequireComponent(typeof(PedestalGroup))]
    public class PedestalGroupSave : SaveObject
    {
        [Serializable]
        public class Data
        {
            public PedestalData[] PedestalDatas;

            public Data(Data data = null)
            {
                PedestalDatas = data?.PedestalDatas;
            }

            [Serializable]
            public struct PedestalData
            {
                public int Index;
                public string ItemID;

                public bool Equals(PedestalData data)
                {
                    return data.Index == Index && data.ItemID == ItemID;
                }

                public PedestalData(int index, string itemId)
                {
                    Index = index;
                    ItemID = itemId;
                }
            }
        }

        private Data _currentData = new Data();
        private Data _newData = new Data();
        private PedestalGroup _pedestalGroup
        {
            get
            {
                if (!m_pedestalGroup) m_pedestalGroup = GetComponent<PedestalGroup>();
                return m_pedestalGroup;
            }
        }
        private PedestalGroup m_pedestalGroup;

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateData(ref _currentData);
        }

        public override void SaveData()
        {
            if (!SaveSystem.Instance && SaveSystem.Instance.Progress == null) return;
            UpdateData(ref _newData);
            if (!IsSameValue(_newData.PedestalDatas, _currentData.PedestalDatas))
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
                _currentData = new Data(data);
                _pedestalGroup.LoadData(null, _currentData);
            }
        }

        private void UpdateData(ref Data data)
        {
            List<Data.PedestalData> temp = new List<Data.PedestalData>();
            for (int i = 0; i < _pedestalGroup.Pedestals.Length; i++)
            {
                if (_pedestalGroup.Pedestals[i].IsActive) temp.Add(new Data.PedestalData(i, _pedestalGroup.Pedestals[i].CurrentItemName));
            }
            data.PedestalDatas = temp.ToArray();
        }

        private bool IsSameValue(Data.PedestalData[] first, Data.PedestalData[] second)
        {
            if (first.Length != second.Length) return false;
            for (int i = 0; i < first.Length; i++)
            {
                if (!first[i].Equals(second[i])) return false;
            }
            return true;
        }
    }
}