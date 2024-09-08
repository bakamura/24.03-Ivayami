
using UnityEngine;

namespace Ivayami.Save {
    [System.Serializable]
    public class SaveProgress {

        public byte id;
        public string lastPlayedDate;
        
        public string[] inventory;
        public int pointId;
        public string lastProgressType;
        public SerializableDictionary<string, int> progress = new SerializableDictionary<string, int>();
        public SerializableDictionary<string, string> saveObjects = new SerializableDictionary<string, string>();

        public SaveProgress(byte id) {
            this.id = id;
        }

        public void SaveProgressOfType(string type, int amount) {
            if (progress.ContainsKey(type)) {
                if (progress[type] < amount) progress[type] = amount;
                else Debug.LogWarning("The value is smaller then the current progress step");
            }
            else progress.Add(type, amount);
        }

        public int GetProgressOfType(string type) {
            if (progress.ContainsKey(type)) return progress[type];
            else return 0;
        }

        public void RecordSaveObject(string id, object data)
        {
            if (saveObjects.ContainsKey(id))
                saveObjects[id] = JsonUtility.ToJson(data);
            else saveObjects.Add(id, JsonUtility.ToJson(data));
        }

        public bool GetSaveObjectOfType<T>(string id, out T data)
        {
            if (saveObjects.ContainsKey(id))
            {
                data = JsonUtility.FromJson<T>(saveObjects[id]);
                return true;
            }
            //return JsonUtility.FromJson<T>(saveObjects[id]);
            else
            {
                Logger.Log(LogType.Save, $"The object {id} has not been saved yet");
                data = default(T);
                return false;
                //data = null;
                //return data;
            }
        }
    }
}