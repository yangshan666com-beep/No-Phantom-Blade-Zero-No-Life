using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;

public class CreateLevel : MonoBehaviour
{
    [Serializable]
    public class LevelEntry
    {
        public string displayName = "第一关";
        public GameObject layoutPrefab;
    }
    [Header("关卡列表")]
    public List<LevelEntry> levels = new List<LevelEntry>();

    [Header("生成物根节点名称")]
    public string levelRootName = "[Level]";

    Transform levelRoot;
    public int CurrentLevelIndex { get; private set; } = 0;

    void OnEnable()                                          // 订阅事件
    {GameSignals.LevelRequested += Create;}
    void OnDisable()                                         // 退订事件
    {GameSignals.LevelRequested -= Create;}

    void Create(int index)
    {
        LoadLevel(index);
    
    }

    public void LoadLevel(int index)                      // 生成第 index 关
    {
          if (levels.Count == 0)
        {
            Debug.LogError("[CreateLevel] levels 里一个关卡都没配");
            return;
        }

        // UI 按钮可能比配置的关卡多 → 钳制到合法范围，避免直接黑屏
        int safe = Mathf.Clamp(index, 0, levels.Count - 1);
        if (safe != index)
        {
            Debug.LogWarning("[CreateLevel] 关卡索引 " + index + " 越界（共 " + levels.Count
                             + " 关），改用第 " + (safe + 1) + " 关");
        }
        index = safe;

        LevelEntry config = levels[index];
        if (config == null || config.layoutPrefab == null)
        {
            Debug.LogError("[CreateLevel] 第 " + (index + 1) + " 关的 LevelEntry 或 layoutPrefab 是空的");
            return;
        }

        CurrentLevelIndex = index;          // ← 检查全部通过后才赋值

        ClearLevel();

        GameObject layout = Instantiate(config.layoutPrefab, Vector3.zero, Quaternion.identity, levelRoot);
        layout.name = "Layout_" + index;

        GameSignals.MarkLevelReady(index);

        Debug.Log("[CreateLevel] 第 " + (index + 1) + " 关「" + config.displayName + "」生成完毕");
    }

    void Start()
    {
        // 游戏场景启动时，加载菜单里选中的关卡
        // （直接从编辑器 Play 本场景时，SelectedIndex 默认 0 = 第一关）
        int index = Mathf.Max(0, LevelSelection.SelectedIndex);
        Debug.Log("[CreateLevel] 场景启动，加载第 " + (index + 1) + " 关");
        LoadLevel(index);
    }
    // ---------- 清空 ----------
    void ClearLevel()
    {
        if (levelRoot != null)
        {
            levelRoot.gameObject.SetActive(false);   // 先禁用，避免这一帧还在跑逻辑
            Destroy(levelRoot.gameObject);
        }

        levelRoot = new GameObject(levelRootName).transform;
    }
}