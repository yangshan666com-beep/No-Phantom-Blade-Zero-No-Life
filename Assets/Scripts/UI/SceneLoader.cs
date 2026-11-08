using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Single entry point for scene transitions. All UI code must call this, never SceneManager directly.
/// </summary>
public static class SceneLoader
{
    public static bool IsLoading { get; private set; }

    private class Runner : MonoBehaviour { }
    private static Runner runner;

    public static void LoadAsync(string sceneName)
    {
        if (IsLoading)
        {
            Debug.LogWarning("[SceneLoader] A load is already in progress.");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneLoader] Scene name is empty.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[SceneLoader] Scene '{sceneName}' is not in Build Settings.");
            return;
        }

        IsLoading = true;

        if (runner == null)
        {
            var go = new GameObject("[SceneLoader]");
            Object.DontDestroyOnLoad(go);
            runner = go.AddComponent<Runner>();
        }

        runner.StartCoroutine(LoadRoutine(sceneName));
    }

    private static IEnumerator LoadRoutine(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName);

        if (op == null)
        {
            Debug.LogError($"[SceneLoader] Failed to start loading '{sceneName}'.");
            IsLoading = false;
            yield break;
        }

        while (!op.isDone)
            yield return null;

        Time.timeScale = 1f;
        IsLoading = false;
    }
}
