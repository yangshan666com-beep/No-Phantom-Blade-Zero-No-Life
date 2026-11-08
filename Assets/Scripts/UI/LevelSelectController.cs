using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main menu -> level select -> game, with cross-fade.
/// All six level buttons load the SAME gameplay scene and pass a spawn index
/// through LevelSelection.SelectedIndex.
/// </summary>
public class LevelSelectController : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("MainPanel 上的 CanvasGroup，淡出用")]
    [SerializeField] private CanvasGroup mainGroup;

    [Tooltip("LevelSelectPanel 根物体，初始隐藏")]
    [SerializeField] private GameObject levelPanel;

    [Tooltip("LevelSelectPanel 上的 CanvasGroup，淡入用")]
    [SerializeField] private CanvasGroup levelGroup;

    [Header("Flow")]
    [Tooltip("六个按钮都进这个场景")]
    [SerializeField] private string gameSceneName = "Main";

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.2f;

    private bool busy;
    private bool levelOpen;

    // ================= buttons =================

    // Start 按钮
    public void OpenLevelSelect()
    {
        if (busy || levelOpen)
            return;

        levelOpen = true;
        StartCoroutine(FadeRoutine(true));
    }

    // LevelSelectPanel 里的 Back 按钮
    public void CloseLevelSelect()
    {
        if (busy || !levelOpen)
            return;

        levelOpen = false;
        StartCoroutine(FadeRoutine(false));
    }

    // 关卡按钮：index 是复活点编号（0 起），全部加载同一个 Main 场景
    public void SelectLevel(int index)
    {
        LevelSelection.SelectedIndex = Mathf.Max(0, index);

        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogWarning("[LevelSelect] gameSceneName is empty.");
            return;
        }

        Debug.Log($"[LevelSelect] Load {gameSceneName} at spawn point {LevelSelection.SelectedIndex}");
        SceneLoader.LoadAsync(gameSceneName);
    }

    // ================= fade =================

    private IEnumerator FadeRoutine(bool toLevels)
    {
        busy = true;

        if (mainGroup == null || levelGroup == null || levelPanel == null)
        {
            Debug.LogWarning("[LevelSelect] References missing.");
            busy = false;
            yield break;
        }

        if (toLevels)
        {
            levelPanel.SetActive(true);
            levelGroup.alpha = 0f;
            levelGroup.blocksRaycasts = false;
            levelGroup.interactable = false;
        }

        float fromMain = mainGroup.alpha;
        float fromLevel = levelGroup.alpha;
        float toMain = toLevels ? 0f : 1f;
        float toLevel = toLevels ? 1f : 0f;

        float d = Mathf.Max(0.01f, fadeDuration);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / d;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

            mainGroup.alpha = Mathf.LerpUnclamped(fromMain, toMain, k);
            levelGroup.alpha = Mathf.LerpUnclamped(fromLevel, toLevel, k);

            yield return null;
        }

        mainGroup.alpha = toMain;
        mainGroup.blocksRaycasts = !toLevels;
        mainGroup.interactable = !toLevels;

        levelGroup.alpha = toLevel;
        levelGroup.blocksRaycasts = toLevels;
        levelGroup.interactable = toLevels;

        if (toLevels)
        {
            mainGroup.gameObject.SetActive(false);
        }
        else
        {
            mainGroup.gameObject.SetActive(true);
            levelPanel.SetActive(false);
        }

        busy = false;
    }
}
