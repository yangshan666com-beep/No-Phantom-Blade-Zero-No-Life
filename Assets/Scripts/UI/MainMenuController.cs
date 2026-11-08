using UnityEngine;

/// <summary>
/// Menu-side implementation of IGameFlow. Lives on the MainMenuPanel prefab.
/// Buttons call these public methods through UnityEvents; no scene references required.
/// </summary>
public class MainMenuController : MonoBehaviour, IGameFlow
{
    [Header("Scenes (must exist in Build Settings)")]
    //please change the scene names to match your project
    [SerializeField] private string gameSceneName = "Main";
    [SerializeField] private string menuSceneName = "UIstart";

    public void StartNewGame()
    {
        SceneLoader.LoadAsync(gameSceneName);
    }

    public void ContinueGame()
    {
        // No save system yet: continue behaves like a fresh start.
        SceneLoader.LoadAsync(gameSceneName);
    }

    public void ReturnToMenu()
    {
        SceneLoader.LoadAsync(menuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
