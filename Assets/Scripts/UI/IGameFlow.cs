/// <summary>
/// Game flow contract used by the UI module. Implemented by menu/pause controllers.
/// </summary>
public interface IGameFlow
{
    void StartNewGame();
    void ContinueGame();
    void ReturnToMenu();
    void QuitGame();
}
