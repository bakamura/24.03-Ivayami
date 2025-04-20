using System.Collections.Generic;
using UnityEngine;
using System;
using Ivayami.Puzzle;

namespace Ivayami.Save
{
    [RequireComponent(typeof(RotatingPedestalPuzzle))]
    public sealed class RotatingPedestalPuzzleSave : SaveObject
    {
        [Serializable]
        public class Data
        {
            public float[] LayersRotations;
            public ObjectInfo[] PuzzleObjectsWithItemsIndex;

            public Data(Data data = null)
            {
                LayersRotations = data?.LayersRotations;
                PuzzleObjectsWithItemsIndex = data?.PuzzleObjectsWithItemsIndex;
            }

            [Serializable]
            public struct ObjectInfo
            {
                public int LayerIndex;
                public int ObjectIndex;

                public bool Equals(ObjectInfo info)
                {
                    return info.LayerIndex == LayerIndex && info.ObjectIndex == ObjectIndex;
                }

                public ObjectInfo(int layerIndex, int objectIndex)
                {
                    LayerIndex = layerIndex;
                    ObjectIndex = objectIndex;
                }
            }
        }
        private Data _currentData = new Data();
        private Data _newData = new Data();
        private RotatingPedestalPuzzle _puzzle
        {
            get
            {
                if (!m_puzzle)
                {
                    m_puzzle = GetComponent<RotatingPedestalPuzzle>();
                    m_puzzle.Setup();
                }
                return m_puzzle;
            }
        }
        private RotatingPedestalPuzzle m_puzzle;

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateData(ref _currentData);
        }

        public override void LoadData()
        {
            if (!SaveSystem.Instance && SaveSystem.Instance.Progress == null) return;
            if (SaveSystem.Instance.Progress.GetSaveObjectOfType<Data>(ID, out Data data))
            {
                _puzzle.LoadData(data);
                _currentData = new Data(data);
            }
        }

        public override void SaveData()
        {
            if (!SaveSystem.Instance && SaveSystem.Instance.Progress == null) return;
            UpdateData(ref _newData);
            bool willSave = false;

            if (!IsSameRotation(_newData.LayersRotations, _currentData.LayersRotations))
            {
                willSave = true;
                _currentData.LayersRotations = _newData.LayersRotations;
            }

            if (!IsSamePuzzleObjectsIndex(_newData.PuzzleObjectsWithItemsIndex, _currentData.PuzzleObjectsWithItemsIndex))
            {
                willSave = true;
                _currentData.PuzzleObjectsWithItemsIndex = _newData.PuzzleObjectsWithItemsIndex;
            }

            if (willSave)
                SaveSystem.Instance.Progress.RecordSaveObject(ID, _currentData);
        }

        private void UpdateData(ref Data data)
        {
            data.LayersRotations = new float[_puzzle.RotatingObjects.Length];
            for (int i = 0; i < _puzzle.RotatingObjects.Length; i++)
            {
                data.LayersRotations[i] = _puzzle.RotatingObjects[i].Transform.localEulerAngles.y;
            }

            List<Data.ObjectInfo> puzzleObjectsIndexs = new List<Data.ObjectInfo>();
            for (int i = 0; i < _puzzle.PuzzleObjects.Count; i++)
            {
                for (int a = 0; a < _puzzle.PuzzleObjects[i].Length; a++)
                {
                    if (_puzzle.PuzzleObjects[i][a].HasItem) puzzleObjectsIndexs.Add(new Data.ObjectInfo(i, a));
                }
            }
            data.PuzzleObjectsWithItemsIndex = puzzleObjectsIndexs.ToArray();
        }

        private bool IsSameRotation(float[] first, float[] second)
        {
            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i]) return false;
            }
            return true;
        }

        private bool IsSamePuzzleObjectsIndex(Data.ObjectInfo[] first, Data.ObjectInfo[] second)
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