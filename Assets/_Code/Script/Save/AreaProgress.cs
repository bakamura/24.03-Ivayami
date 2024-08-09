using UnityEngine;

namespace Ivayami.Save
{
    [CreateAssetMenu(menuName = "ChapterInfo/AreaProgress")]
    public class AreaProgress : ScriptableObject
    {
        [SerializeField, TextArea(1, 3)] private string[] _steps;
        public string Id => name;
        public string[] Steps => _steps;
    }
}