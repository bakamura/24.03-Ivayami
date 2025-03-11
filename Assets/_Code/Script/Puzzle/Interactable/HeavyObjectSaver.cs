using System;
using System.Reflection;
using UnityEngine;
using Ivayami.Save;

namespace Ivayami.Puzzle {
    public class HeavyObjectSaver : SaveObject {

        [Header("References")]

        [SerializeField, ReadOnly] private Transform _heavyObjectParent;

        [Serializable]
        public class Data {
            public string currentObjectName;
        }

        private Data _dataCurrent;

        public override void LoadData() {
            if (SaveSystem.Instance?.Progress != null) {
                if (SaveSystem.Instance.Progress.GetSaveObjectOfType<Data>(ID, out Data data)) {
                    _dataCurrent = data;
                    if (_heavyObjectParent.childCount > 0 && _heavyObjectParent.GetChild(0).name != _dataCurrent.currentObjectName) {
                        Destroy(_heavyObjectParent.GetChild(0));
                        if (!string.IsNullOrEmpty(_dataCurrent.currentObjectName)) Instantiate((Resources.Load($"HeavyObjects/{data.currentObjectName}") as GameObject), _heavyObjectParent);
                    }
                }
            }
        }

        public override void SaveData() {
            if (SaveSystem.Instance?.Progress != null) {
                if (TryGetComponent(out HeavyObjectPlacement placement)) {
                    if (_dataCurrent.currentObjectName != placement.HeavyObjectCurrentName) {
                        _dataCurrent.currentObjectName = placement.HeavyObjectCurrentName;
                        SaveSystem.Instance.Progress.RecordSaveObject(ID, _dataCurrent);
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            FieldInfo fieldInfo = typeof(HeavyObjectPlacement).GetField("_placementPos", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null) {
                if (TryGetComponent(out HeavyObjectPlacement placement)) _heavyObjectParent = fieldInfo.GetValue(placement) as Transform;
                else Debug.LogError($"Heavy Saver Couldn't get HeavyPlacement in '{name}'");
            }
            else Debug.LogError($"Heavy Saver couldn't get object from other script through reflection!");
#endif
        }
    }
