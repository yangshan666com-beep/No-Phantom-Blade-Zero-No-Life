/// <summary>
/// Carries the selected level (spawn point index) across the scene load.
/// Main.cs reads SelectedIndex after loading and teleports the player.
/// Static state resets on domain reload (editor) and on app start.
/// </summary>
public static class LevelSelection
{
    /// <summary>0-based spawn point index chosen in the level select panel.</summary>
    public static int SelectedIndex { get; set; }

    public static void Reset()
    {
        SelectedIndex = 0;
    }
}
