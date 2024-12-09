using UnityEngine;

namespace Ivayami.Dialogue
{
    [CreateAssetMenu(menuName = "DialogueSystem/ActorsTable")]
    public class DialogueActor : ScriptableObject
    {
        public string[] Actors;
    }
}