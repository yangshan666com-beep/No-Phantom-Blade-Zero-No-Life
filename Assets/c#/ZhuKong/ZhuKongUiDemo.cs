using UnityEngine;

public class ZhuKongUiDemo : MonoBehaviour
{
    public LevelConfig config;

    string message = "";

    void OnEnable()
    {
        GameSignals.LevelReady += OnLevelReady;
    }

    void OnDisable()
    {
        GameSignals.LevelReady -= OnLevelReady;
    }

    void OnLevelReady(int index)
    {
        if (config != null)
        {
            message = "关卡已生成：" + config.displayName + "  (index " + index + ")";
        }
    }

    void OnGUI()
    {
        if (message.Length == 0)
        {
            return;
        }

        GUIStyle style = new GUIStyle();
        style.fontSize = 22;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(20f, 18f, 900f, 40f), message, style);
    }
}
