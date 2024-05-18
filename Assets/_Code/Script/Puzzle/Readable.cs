using UnityEngine;

[CreateAssetMenu(menuName = "Item/Readable")]
public class Readable : ScriptableObject {

    [field: SerializeField] public string Content { get; private set; }

}
