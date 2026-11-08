using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuShiHua
{
    [Header("帧率")]
    [Tooltip("0 = 平台默认；-1 = 极限;120 = 固定上限。")]
    public int targetFrameRate = 120;
    [Header("更新频率")]
    [Tooltip("默认 0.02 = 每秒 50 次 FixedUpdate。")]
    public float fixedDeltaTime = 0.02f;

    // Start is called before the first frame update
    [Header("关卡入口")]
    [Tooltip("直接在编辑器里 Play 本场景时进入第几关（从菜单进入时以菜单为准）")]
    public int GuanQia = 0;

    // ===== 应用级设置：不挂在任何场景上，游戏启动时执行一次 =====
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplyAppSettings()
    {
        QualitySettings.vSyncCount = 0;
        Time.fixedDeltaTime = 0.02f;
        Application.targetFrameRate = 120;
    }
    
    // Update is called once per frame
}
