using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Ivayami.Dialogue
{
    [CreateAssetMenu(menuName = "DialogueSystem/ActorsTable")]
    public class DialogueActor : ScriptableObject
    {
        public Actor[] Actors;

        [Serializable]
        public struct Actor
        {
            [ReadOnly] public ushort ID;
            public DisplayNameInfo[] DisplayNames;
        }
        [Serializable]
        public struct DisplayNameInfo
        {
            [ReadOnly] public string Language;
            public string Name;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < Actors.Length; i++)
            {
                Actors[i].ID = (ushort)i;
                Debug.Log(LocalizationSettings.AvailableLocales.Locales.Count);
                if (Actors[i].DisplayNames.Length != LocalizationSettings.AvailableLocales.Locales.Count)
                {
                    Actor[] temp = Actors;
                    Array.Resize(ref Actors[i].DisplayNames, LocalizationSettings.AvailableLocales.Locales.Count);
                    if(temp.Length > Actors.Length)
                    {
                        for (int a = 0; a < Actors[i].DisplayNames.Length; a++)
                        {

                        }
                    }
                    for (int a = 0; a < Actors[i].DisplayNames.Length; a++)
                    {
                        Actors[i].DisplayNames[a].Language = LocalizationSettings.AvailableLocales.Locales[a].LocaleName;
                    }
                }

            }
        }
#endif
    }
}