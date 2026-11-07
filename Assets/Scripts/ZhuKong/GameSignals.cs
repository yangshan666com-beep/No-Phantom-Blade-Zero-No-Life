using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;  

public static class GameSignals
{
    public static event Action<int> LevelRequested;
    //创建第 index 关的事件表

    public static event Action<int> LevelReady;
    //创建第 index 关完成的事件表

    public static event Action<Player1> PlayerSpawned;
    //主角生成完毕的事件表

    public static void RequestLevel(int index)
    {
        // 先存局部变量：如果最后一个订阅者在回调里退订，直接 Invoke 会抛空引用
        Action<int> handler = LevelRequested;
        if (handler != null) handler(index);
    }

    public static void MarkLevelReady(int index)
    {
        Action<int> handler = LevelReady;
        if (handler != null) handler(index);
    }
    public static void MarkPlayerSpawned(Player1 player)
    {
        Action<Player1> handler = PlayerSpawned;
        if (handler != null) handler(player);
    }
    /// 关闭 Domain Reload 时（Enter Play Mode Options），静态字段不会自动清空，
    /// 会导致上一次运行残留的订阅。这个钩子专门用来兜底。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        LevelRequested = null;
        LevelReady = null;
        PlayerSpawned = null;
    }
}
