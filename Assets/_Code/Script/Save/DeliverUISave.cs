using UnityEngine;
using Ivayami.Puzzle;
using System;
using System.Linq;

namespace Ivayami.Save
{
    [RequireComponent(typeof(DeliverUI))]
    public class DeliverUISave : SaveObject
    {
        [Serializable]
        public class Data
        {
            public string[] ItemsDeliveredIDs;

            public Data(Data data = null)
            {
                ItemsDeliveredIDs = data?.ItemsDeliveredIDs;
            }
        }

        private Data _currentData = new Data();
        private Data _newData = new Data();
        private DeliverUI _deliverUI
        {
            get
            {
                if (!m_deliverUI) m_deliverUI = GetComponent<DeliverUI>();
                return m_deliverUI;
            }
        }
        private DeliverUI m_deliverUI;
        protected override void OnEnable()
        {
            base.OnEnable();
            _currentData.ItemsDeliveredIDs = _deliverUI.ItemsDelivered.Select(x => x.name).ToArray();
        }

        public override void SaveData()
        {
            if (!SaveSystem.Instance && SaveSystem.Instance.Progress == null) return;            
            _newData.ItemsDeliveredIDs = _deliverUI.ItemsDelivered.Select(x => x.name).ToArray();

            if (!IsSameValue(_newData, _currentData))
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
                _deliverUI.LoadData(_currentData);
            }
        }

        private bool IsSameValue(Data first, Data second)
        {
            if (first.ItemsDeliveredIDs.Length != second.ItemsDeliveredIDs.Length) return false;
            for (int i = 0; i < first.ItemsDeliveredIDs.Length; i++)
            {
                if (first.ItemsDeliveredIDs[i] != second.ItemsDeliveredIDs[i]) return false;
            }
            return true;
        }
    }
}