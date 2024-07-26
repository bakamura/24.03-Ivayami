using UnityEngine;
using IngameDebugConsole;
using Ivayami.Player;

public static class PlayerTerminalCommands {

    [ConsoleMethod("TogglePlayerMovement", "Toggles Player's Movement", "canMove")]
    public static void TogglePlayerMovement(bool canMove) {
        PlayerMovement.Instance.ToggleMovement(canMove);
    }

    [ConsoleMethod("ChangeInputMap", "Changes the current input map", "mapName")]
    public static void ChangeInputMap(string mapName) {
        PlayerActions.Instance.ChangeInputMap(mapName);
    }

    [ConsoleMethod("AddPlayerStress", "Adds stress to Player", "amount")]
    public static void AddPlayerStress(float amount) {
        PlayerStress.Instance.AddStress(amount);
    }

    [ConsoleMethod("SetMinPlayerStress", "Sets min stress to Player", "amount")]
    public static void SetMinPlayerStress(float amount) {
        PlayerStress.Instance.SetStressMin(amount);
    }

}
