using UnityEngine;
using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

namespace Ivayami.Dialogue
{
    [CreateAssetMenu(menuName = "Ivayami/UI/Dialogue")]
    public class Dialogue : ScriptableObject
    {
        [HideInInspector] public int ID => GetInstanceID();
        public DialogueEvent[] dialogue;
        public string onEndEventId;
        [Serializable]
        public struct DialogueEvent
        {
            public LocalizedString AnnouncerName;
#if UNITY_EDITOR
            public Speech[] Speeches;
            public string FilterTags;
#endif
            public string EventId;
            [Min(0f), Tooltip("Wait for this time to continue dialogue. Will not continue if a cutscene is playing or value is 0")] public float FixedDurationInSpeech;
        }

#if UNITY_EDITOR
        private bool _hasBeenInstantiated;
        private int _previousSize;
        private void OnValidate()
        {
            if (!_hasBeenInstantiated)
            {
                _hasBeenInstantiated = true;
                DialogueEventsInspector.UpdateDatabaseOnInspectorSelected = true;
            }
            if (dialogue == null) return;
            if (_previousSize > 0 && _previousSize < dialogue.Length)
            {
                dialogue[dialogue.Length - 1].EventId = null;
                dialogue[dialogue.Length - 1].FilterTags = null;
                dialogue[dialogue.Length - 1].FixedDurationInSpeech = 0;
            }
            int languagesCount = LocalizationSettings.AvailableLocales.Locales.Count;
            for (int i = 0; i < dialogue.Length; i++)
            {
                if (dialogue[i].Speeches == null || (dialogue[i].Speeches.Length != languagesCount && languagesCount > 0))
                {
                    if (dialogue[i].Speeches == null) dialogue[i].Speeches = new Speech[languagesCount];
                    Array.Resize(ref dialogue[i].Speeches, languagesCount);
                    for (int a = 0; a < dialogue[i].Speeches.Length; a++)
                    {
                        dialogue[i].Speeches[a].Language = LocalizationSettings.AvailableLocales.Locales[a].LocaleName;
                    }
                }
            }
            _previousSize = dialogue.Length;
        }

        private void OnDestroy()
        {
            DialogueEventsInspector.UpdateDatabaseOnInspectorSelected = true;
        }
#endif
    }
}