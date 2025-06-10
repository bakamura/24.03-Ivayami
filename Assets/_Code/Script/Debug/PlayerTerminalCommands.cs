using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IngameDebugConsole;
using Ivayami.Player;
using Ivayami.UI;
using Ivayami.debug;
using Ivayami.Player.Ability;

public static class PlayerTerminalCommands
{

    private static List<InventoryItem> _allItemsList = Resources.LoadAll<InventoryItem>("Items").ToList();
    private static AbilityGiverRef _abilityGiver;

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

    [ConsoleMethod("UpdateAutoRegenStress", "", "isActive")]
    public static void UpdateAutoRegenStress(bool isActive)
    {
        PlayerStress.Instance.UpdateAutoRegenerateStress(isActive);
        Debug.Log($"Auto Regen is now {isActive}");
    }

    [ConsoleMethod("ReleasePlayerControls", "")]
    public static void ReleasePlayerControls()
    {
        PlayerMovement.Instance.RemoveAllBlockers();
        Pause.Instance.RemoveAllBlockers();
        PlayerActions.Instance.ChangeInputMap("Player");
        Debug.Log($"Release Player Complete");
    }

    [ConsoleMethod("ChangePlayerRunSpeed", "", "value")]
    public static void ChangePlayerRunSpeed(float value)
    {
        PlayerMovement.Instance.ChangeRunSpeed(value);
        Debug.Log($"New run speed {value}");
    }

    [ConsoleMethod("FlyMode", "", "isActive")]
    public static void FlyMode(bool isActive/*, float speed*/)
    {
        PlayerFlyModeInputs.Instance.UpdateInputs(isActive/*, speed*/);
        Debug.Log($"Fly Mode is now {isActive}");
    }

    [ConsoleMethod("GiveItem", "", "itemID", "amount")]
    public static void GiveItem(string itemID, int amount = 1)
    {
        InventoryItem result = _allItemsList.Find(x => x.name.ToUpper() == itemID.ToUpper());
        if (result != null)
        {
            for(int i = 0; i < amount; i++) PlayerInventory.Instance.AddToInventory(result);
            Debug.Log($"The item {itemID} has been added to inventory");
        }
        else Debug.LogWarning($"The item: {itemID} doesn't exist");
    }

    [ConsoleMethod("GiveAllTapes", "")]
    public static void GiveAllTapes()
    {
        InventoryItem[] tapes = Resources.LoadAll<InventoryItem>("Items/Tapes");
        for(int i = 0; i < tapes.Length; i++)
        {
            PlayerInventory.Instance.AddToInventory(tapes[i]);
        }
    }

    [ConsoleMethod("GiveAbility", "", "abilityName")]
    public static void GiveAbility(string abilityName)
    {
        if (!_abilityGiver) _abilityGiver = DebugLogManager.Instance.GetComponent<AbilityGiverRef>();
        char[] temp = abilityName.ToCharArray();
        temp[0] = char.ToUpper(temp[0]);
        _abilityGiver.GiveAbility(new string(temp));
    }

    [ConsoleMethod("ToggleRun", "", "canRun")]
    public static void ToggleRun(bool canRun)
    {
        PlayerMovement.Instance.AllowRun(canRun);
        Debug.Log($"Run is now {canRun}");
    }

}
