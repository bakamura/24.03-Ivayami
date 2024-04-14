using UnityEngine;
using Paranapiacaba.Player;
public class Playtest : MonoBehaviour
{
    private void Start()
    {
        PlayerMovement.Instance.ToggleMovement(true);
        PlayerActions.Instance.ChangeInputMap("Player");
    }
}
