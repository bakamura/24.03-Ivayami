using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IngameDebugConsole;
using Ivayami.Player;

public static class PlayerTerminalCommands
{

    private static List<InventoryItem> _allItemsList = Resources.LoadAll<InventoryItem>("Items").ToList();

    [ConsoleMethod("TogglePlayerMovement", "Toggles Player's Movement", "key", "canMove")]
    public static void TogglePlayerMovement(string key, bool canMove)
    {
        PlayerMovement.Instance.ToggleMovement(key, canMove);
    }

    [ConsoleMethod("ChangeInputMap", "Changes the current input map", "mapName")]
    public static void ChangeInputMap(string mapName)
    {
        PlayerActions.Instance.ChangeInputMap(mapName);
    }

    [ConsoleMethod("AddPlayerStress", "Adds stress to Player", "amount")]
    public static void AddPlayerStress(float amount)
    {
        PlayerStress.Instance.AddStress(amount);
    }


    [ConsoleMethod("GiveItem", "", "itemID")]
    public static void GiveItem(string itemID)
    {
        InventoryItem result = _allItemsList.Find(x => x.name.ToUpper() == itemID.ToUpper());
        if (result != null)
        {
            PlayerInventory.Instance.AddToInventory(result);
            Debug.Log($"The item {itemID} has been added to inventory");
        }
        else Debug.LogWarning($"The item: {itemID} doesn't exist");
    }

    [ConsoleMethod("ToggleRun", "", "canRun")]
    public static void ToggleRun(bool canRun)
    {
        PlayerMovement.Instance.AllowRun(canRun);
        Debug.Log($"Run is now {canRun}");
    }

}
