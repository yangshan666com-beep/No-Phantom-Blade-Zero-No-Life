using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置面板：挂在 Manager 上。
/// 打开：主菜单左移淡出，设置面板从右滑入并淡入；关闭：反向执行。
/// </summary>
public class settingpanel : MonoBehaviour
{
    [Header("面板")]
    [Tooltip("设置面板根物体")]
    [SerializeField] private GameObject settingPanel;

    [Tooltip("主菜单根物体（标题 + 开始/设置/退出三个按钮）")]
    [SerializeField] private RectTransform mainPanel;

    [Tooltip("MainPanel 上的 CanvasGroup，用来做淡化")]
    [SerializeField] private CanvasGroup mainGroup;

    [Tooltip("settingPanel 上的 CanvasGroup，用来做淡入淡出")]
    [SerializeField] private CanvasGroup settingsGroup;

    [Header("滑动动画")]
    [Tooltip("滑动距离，建议不小于画布宽度（1080p 用 1920 左右）")]
    [SerializeField] private float slideDistance = 1400f;

    [Tooltip("单次滑动时长（秒）")]
    [SerializeField] private float duration = 0.35f;

    [Tooltip("设置面板起点距离的倍数，>1 可以让它从更远处进入，避免与主菜单交叉")]
    [SerializeField] private float settingsOffsetMultiplier = 1.6f;

    [Header("设置项")]
    [Tooltip("音量滑条，Min = -80，Max = 20")]
    [SerializeField] private Slider VolumeSlider;

    [Tooltip("静音开关：默认不勾（有声音），勾上 = 静音")]
    [SerializeField] private Toggle VolumeToggle;

    [Tooltip("全屏开关")]
    [SerializeField] private Toggle FullscreenToggle;

    private const string KeyVolume = "settings.volume";
    private const string KeyFullscreen = "settings.fullscreen";
    private const string KeyMuted = "settings.muted";

    private Vector2 mainHome;
    private Vector2 settingsHome;
    private bool animating;
    private bool homeReady;

    // 全屏开关的「真实状态」。
    // 不能直接读 Screen.fullScreen：编辑器里它永远是 false。
    private bool fullscreenOn;
    private bool muted;
    private float volumeValue = 1f;

    // ================= 生命周期 =================

    void Awake()
    {
        if (mainPanel != null)
            mainHome = mainPanel.anchoredPosition;

        if (settingPanel != null)
        {
            var rt = settingPanel.GetComponent<RectTransform>();
            if (rt != null)
                settingsHome = rt.anchoredPosition;
        }

        homeReady = mainPanel != null && settingPanel != null;
    }

    void Start()
    {
        LoadSettings();

        if (settingPanel != null)
            settingPanel.SetActive(false);

        if (mainPanel != null)
            mainPanel.anchoredPosition = mainHome;

        if (mainGroup != null)
            mainGroup.alpha = 1f;

        if (settingsGroup != null)
        {
            settingsGroup.alpha = 0f;
            settingsGroup.blocksRaycasts = false;
            settingsGroup.interactable = false;
        }
    }

    // ================= 按钮入口 =================

    // 绑到 settingbutton 的 OnClick
    public void Open()
    {
        if (animating)
            return;

        RefreshUI();
        StartCoroutine(Switch(true));
    }

    // 绑到 Back 的 OnClick
    public void Close()
    {
        if (animating)
            return;

        StartCoroutine(Switch(false));
    }

    /// <summary>设置面板当前是否可见</summary>
    public bool IsOpen
    {
        get { return settingPanel != null && settingPanel.activeSelf; }
    }

    /// <summary>不做动画，立即回到"主菜单"状态。外部强制复位用（比如从暂停菜单进入）。</summary>
    public void ResetToMenu()
    {
        StopAllCoroutines();
        animating = false;

        if (settingPanel != null)
            settingPanel.SetActive(false);

        if (mainPanel != null)
            mainPanel.anchoredPosition = mainHome;

        if (settingPanel != null)
        {
            var rt = settingPanel.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = settingsHome + new Vector2(GetSettingsOffset(), 0f);
        }

        if (mainGroup != null)
        {
            mainGroup.alpha = 1f;
            mainGroup.blocksRaycasts = true;
            mainGroup.interactable = true;
        }

        if (settingsGroup != null)
        {
            settingsGroup.alpha = 0f;
            settingsGroup.blocksRaycasts = false;
            settingsGroup.interactable = false;
        }
    }

    private float GetSettingsOffset()
    {
        return slideDistance * Mathf.Max(1f, settingsOffsetMultiplier);
    }

    // ================= 滑动 + 淡化 =================

    IEnumerator Switch(bool toSettings)
    {
        if (settingPanel == null || mainPanel == null)
        {
            Debug.LogWarning("[settingpanel] settingPanel 或 mainPanel 未赋值，已跳过滑动动画。");
            if (settingPanel != null)
                settingPanel.SetActive(toSettings);
            yield break;
        }

        var sp = settingPanel.GetComponent<RectTransform>();
        if (sp == null)
        {
            Debug.LogWarning("[settingpanel] settingPanel 上缺少 RectTransform。");
            settingPanel.SetActive(toSettings);
            yield break;
        }

        if (!homeReady)
        {
            settingsHome = sp.anchoredPosition;
            mainHome = mainPanel.anchoredPosition;
            homeReady = true;
        }

        animating = true;

        if (toSettings)
        {
            settingPanel.SetActive(true);

            if (settingsGroup != null)
                settingsGroup.alpha = 0f;
        }

        Vector2 mainFrom = mainPanel.anchoredPosition;
        // 主菜单往右滑出
        Vector2 mainTo = toSettings
            ? mainHome + new Vector2(slideDistance, 0f)
            : mainHome;

        // 设置面板的起点比主菜单的终点更远，两者不会交叉
        float settingsOffset = GetSettingsOffset();

        Vector2 setFrom = sp.anchoredPosition;
        Vector2 setTo = toSettings
            ? settingsHome                                    // 滑到屏幕中央
            : settingsHome + new Vector2(settingsOffset, 0f); // 关闭时退回右侧更远处

        // 第一次打开：面板还停在中央，先挪到右边外面
        if (toSettings && setFrom == setTo)
            setFrom = settingsHome + new Vector2(settingsOffset, 0f);

        float mainAlphaFrom = mainGroup != null ? mainGroup.alpha : 1f;
        float mainAlphaTo = toSettings ? 0f : 1f;

        float setAlphaFrom = settingsGroup != null ? settingsGroup.alpha : 1f;
        float setAlphaTo = toSettings ? 1f : 0f;

        if (mainGroup != null)
        {
            mainGroup.blocksRaycasts = true;
            mainGroup.interactable = true;
        }

        if (settingsGroup != null)
        {
            settingsGroup.blocksRaycasts = false;
            settingsGroup.interactable = false;
        }

        float safeDuration = Mathf.Max(0.01f, duration);
        float t = 0f;

        try
        {
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / safeDuration;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

                mainPanel.anchoredPosition = Vector2.LerpUnclamped(mainFrom, mainTo, k);
                sp.anchoredPosition = Vector2.LerpUnclamped(setFrom, setTo, k);

                if (mainGroup != null)
                    mainGroup.alpha = Mathf.LerpUnclamped(mainAlphaFrom, mainAlphaTo, k);

                if (settingsGroup != null)
                    settingsGroup.alpha = Mathf.LerpUnclamped(setAlphaFrom, setAlphaTo, k);

                yield return null;
            }
        }
        finally
        {
            if (mainPanel != null)
                mainPanel.anchoredPosition = mainTo;

            if (sp != null)
                sp.anchoredPosition = setTo;

            if (mainGroup != null)
            {
                mainGroup.alpha = mainAlphaTo;
                mainGroup.blocksRaycasts = !toSettings;
                mainGroup.interactable = !toSettings;
            }

            if (settingsGroup != null)
            {
                settingsGroup.alpha = setAlphaTo;
                settingsGroup.blocksRaycasts = toSettings;
                settingsGroup.interactable = toSettings;
            }

            animating = false;
        }

        if (!toSettings)
        {
            settingPanel.SetActive(false);
            PlayerPrefs.Save();
        }
    }

    // ================= 设置项 =================

    // 绑到 Slider 的 On Value Changed (Single)
    public void OnVolumeChanged(float value)
    {
        volumeValue = value;
        PlayerPrefs.SetFloat(KeyVolume, value);
        ApplyVolume();
    }

    // 绑到 VolumeToggle 的 On Value Changed (Boolean)
    // 默认不勾 -> 有声音；勾上 -> 静音
    public void OnMuteChanged(bool isOn)
    {
        muted = isOn;
        PlayerPrefs.SetInt(KeyMuted, isOn ? 1 : 0);
        ApplyVolume();
    }

    // 把「当前音量 + 静音状态」应用到 AudioListener
    private void ApplyVolume()
    {
        AudioListener.volume = muted ? 0f : volumeValue;
    }

    // 绑到 Toggle 的 On Value Changed (Boolean)
    public void OnFullscreenChanged(bool isOn)
    {
        fullscreenOn = isOn;          // 以这个字段为准
        Screen.fullScreen = isOn;     // 编辑器里无效，打包后才真正生效
        PlayerPrefs.SetInt(KeyFullscreen, isOn ? 1 : 0);
    }

    // ================= 内部 =================

    private void LoadSettings()
    {
        volumeValue = PlayerPrefs.GetFloat(KeyVolume, 1f);
        muted = PlayerPrefs.GetInt(KeyMuted, 0) == 1;
        ApplyVolume();

        if (VolumeSlider != null)
            VolumeSlider.SetValueWithoutNotify(volumeValue);

        if (VolumeToggle != null)
            VolumeToggle.SetIsOnWithoutNotify(muted);

        // 默认值用 PlayerPrefs；没有存档时才参考 Screen.fullScreen
        fullscreenOn = PlayerPrefs.GetInt(KeyFullscreen, Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = fullscreenOn;
        if (FullscreenToggle != null)
            FullscreenToggle.SetIsOnWithoutNotify(fullscreenOn);
    }

    // 只把 UI 同步成真实值，不读 Screen.fullScreen（编辑器里它恒为 false）
    private void RefreshUI()
    {
        if (VolumeSlider != null)
            VolumeSlider.SetValueWithoutNotify(volumeValue);

        if (VolumeToggle != null)
            VolumeToggle.SetIsOnWithoutNotify(muted);

        if (FullscreenToggle != null)
            FullscreenToggle.SetIsOnWithoutNotify(fullscreenOn);
    }
}