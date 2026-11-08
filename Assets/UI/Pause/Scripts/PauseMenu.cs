using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏内暂停菜单：Esc 暂停 / 恢复；暂停界面里的"设置"按钮复用 settingpanel。
/// 挂在游戏场景里一个常驻物体（PauseManager）上。
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("引用")]
    [Tooltip("暂停界面根物体（初始隐藏）")]
    [SerializeField] private GameObject pausePanel;

    [Tooltip("暂停界面上的 CanvasGroup，用于淡入淡出（可选）")]
    [SerializeField] private CanvasGroup pauseGroup;

    [Tooltip("暂停界面里的设置面板（复用 settingpanel 组件）")]
    [SerializeField] private settingpanel settingsScreen;

    [Header("设置")]
    [Tooltip("返回主菜单时加载的场景名")]
    [SerializeField] private string mainMenuSceneName = "UIstart";

    [Tooltip("淡入/淡出时长（秒，不受 timeScale 影响）")]
    [SerializeField] private float fadeDuration = 0.18f;

    [Tooltip("暂停键")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool paused;
    private bool busy;

    // ================= 生命周期 =================

    void Update()
    {
        if (busy)
            return;

        if (!Input.GetKeyDown(pauseKey))
            return;

        // 设置界面开着时，Esc 先关设置，而不是继续游戏
        if (settingsScreen != null && settingsScreen.IsOpen)
        {
            settingsScreen.Close();
            return;
        }

        if (paused)
            Resume();
        else
            Pause();
    }

    void OnDisable()
    {
        // 切场景或对象被禁用时，必须把时间恢复，否则新场景会一直停着
        Time.timeScale = 1f;
    }

    // ================= 对外接口 =================

    // 绑到"继续游戏"按钮
    public void Resume()
    {
        if (!paused || busy)
            return;

        paused = false;
        StartCoroutine(FadeRoutine(1f, 0f, () =>
        {
            Time.timeScale = 1f;
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }));
    }

    // 绑到"设置"按钮
    public void OpenSettings()
    {
        if (settingsScreen != null)
            settingsScreen.Open();
    }

    // 绑到"返回主菜单"按钮
    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ================= 内部 =================

    private void Pause()
    {
        if (paused || busy)
            return;

        paused = true;
        Time.timeScale = 0f;

        // 每次进入暂停都回到"暂停主界面"，避免上次停在设置页
        if (settingsScreen != null)
            settingsScreen.ResetToMenu();

        if (pausePanel != null)
            pausePanel.SetActive(true);

        StartCoroutine(FadeRoutine(0f, 1f));
    }

    private IEnumerator FadeRoutine(float from, float to, Action onDone = null)
    {
        busy = true;

        if (pauseGroup != null)
        {
            float d = Mathf.Max(0.01f, fadeDuration);
            float t = 0f;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / d;
                pauseGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t));
                yield return null;
            }

            pauseGroup.alpha = to;
        }

        busy = false;

        if (onDone != null)
            onDone();
    }
}
