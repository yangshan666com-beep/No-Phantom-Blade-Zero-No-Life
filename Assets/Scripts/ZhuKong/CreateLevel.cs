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
        CurrentLevelIndex = index;
        if (index < 0 || index >= levels.Count)
        {
            Debug.LogError("[CreateLevel] 关卡索引越界: " + index + "（共 " + levels.Count + " 关）");
            return;
        }

        LevelEntry config = levels[index];
        if (config.layoutPrefab == null)
        {
            Debug.LogError("[CreateLevel] 第 " + (index+1) + " 关的 LevelEntry 是空的");
            return;
        }

        ClearLevel();

        GameObject layout = Instantiate(config.layoutPrefab, Vector3.zero, Quaternion.identity, levelRoot);
        layout.name = "Layout_" + index;

        GameSignals.MarkLevelReady(index);                                         // 通知关卡已生成完毕

        Debug.Log("[CreateLevel] 第 " + (index+1) + " 关「" + config.displayName + "」生成完毕");
    }

    // ---------- 清空 ----------
    void ClearLevel()
    {
        if (levelRoot != null)
        {
            Destroy(levelRoot.gameObject);
        }

        levelRoot = new GameObject(levelRootName).transform;
    }
}