using IngameDebugConsole;

public static class ShaderTerminalCommands
{
    [ConsoleMethod("UpdateDithering", "", "value")]
    public static void UpdateDithering(float value)
    {
        PostProcessManager.Instance.ChangeDitheringScale(value);
    }
}
