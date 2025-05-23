using UnityEngine;

namespace Ivayami.Save {
    public sealed class SavePointReference : MonoBehaviour {
 
        [SerializeField] private int _id;

        public void Save() {
            SavePoint.Points[_id].ForceSave();
        }

        public void Teleport() {
            SavePoint.Points[_id].SpawnPoint.Teleport();
        }

    }
}