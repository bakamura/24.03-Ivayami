using IngameDebugConsole;
using UnityEngine.EventSystems;
using UnityEngine;

public static class UtilityTerminalCommands
{
    [ConsoleMethod("ToggleEventSystem", "Toggle Event System")]
    public static void ToggleEventSystem()
    {
        GameObject temp = EventSystem.current.gameObject;
        temp.SetActive(false);
        temp.SetActive(true);
    }
}
