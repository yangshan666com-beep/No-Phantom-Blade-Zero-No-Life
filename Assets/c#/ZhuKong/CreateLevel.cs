using System.Collections.Generic;
using UnityEngine;

public class CreateLevel : MonoBehaviour
{
    [Header("关卡代码列表")]
    public List<LevelConfig> levels = new List<LevelConfig>();

    [Header("生成物根节点名称")]
    public string levelRootName = "[Level]";

    Transform levelRoot;
    Player1 currentPlayer;

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
        if (index < 0 || index >= levels.Count)
        {
            Debug.LogError("[LevelDirector] 关卡索引越界: " + index + "（共 " + levels.Count + " 关）");
            return;
        }

        LevelConfig config = levels[index];
        if (config == null)
        {
            Debug.LogError("[LevelDirector] 第 " + index + " 关的 LevelConfig 是空的");
            return;
        }

        ClearLevel();

        BuildBackground(config);                                                   //生成背景
        BuildBlocks(config);                                                       //生成方块
        SpawnPlayer(config);                                                       //生成主角

        GameSignals.MarkLevelReady(index);                                         // 通知关卡已生成完毕
        if (currentPlayer != null) currentPlayer.SetControlEnabled(true);          // 允许控制角色

        Debug.Log("[LevelDirector] 第 " + index + " 关「" + config.displayName + "」生成完毕");
    }

    // ---------- 清空 ----------
    void ClearLevel()
    {
        if (levelRoot != null)
        {
            Destroy(levelRoot.gameObject);
        }

        levelRoot = new GameObject(levelRootName).transform;
        currentPlayer = null;
    }

    // ---------- 背景 ----------
    void BuildBackground(LevelConfig config)
    {
        if (config.backgroundPrefab == null)
        {
            return;
        }

        Vector2 center = (config.mapMin + config.mapMax) * 0.5f;
        Vector2 size = config.mapMax - config.mapMin;

        GameObject bg = Instantiate(config.backgroundPrefab, center, Quaternion.identity, levelRoot);
        bg.name = "Background";

        // 背景按地图尺寸缩放（前提：预制体是 1x1 单位的图）
        SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            Vector2 spriteSize = sr.sprite.bounds.size;
            if (spriteSize.x > 0f && spriteSize.y > 0f)
            {
                bg.transform.localScale = new Vector3(size.x / spriteSize.x, size.y / spriteSize.y, 1f);
            }
        }
    }

    // ---------- 方块 ----------
    void BuildBlocks(LevelConfig config)
    {
        if (config.blockPrefab == null)
        {
            return;
        }

        for (int i = 0; i < config.blocks.Length; i++)
        {
            LevelConfig.BlockSpec spec = config.blocks[i];
            GameObject go = Instantiate(config.blockPrefab, spec.position, Quaternion.identity, levelRoot);
            go.name = "Block_" + i;
            go.transform.localScale = Vector3.one * spec.scale;

            GravityBlock block = go.GetComponent<GravityBlock>();
            if (block != null)
            {
                block.SetDirection(spec.gravityDirection);
            }
            else
            {
                Debug.LogWarning("[LevelDirector] Block 预制体上没有挂 GravityBlock: " + go.name);
            }
        }
    }

    // ---------- 主角 ----------
    void SpawnPlayer(LevelConfig config)
    {
        if (config.playerPrefab == null)
        {
            Debug.LogError("[LevelDirector] LevelConfig 没指定 playerPrefab");
            return;
        }

        GameObject go = Instantiate(config.playerPrefab, config.playerSpawn, Quaternion.identity, levelRoot);
        go.name = "Player";

        currentPlayer = go.GetComponent<Player1>();
        if (currentPlayer == null)
        {
            Debug.LogError("[LevelDirector] 玩家预制体上没有挂 Player1");
            return;
        }

        // 先锁住输入，等一切就绪再解锁
        currentPlayer.SetControlEnabled(false);

        // 重新绑定场景里的绿圈-后面建议把绿圈改了
        GravityCursor cursor = FindObjectOfType<GravityCursor>();
        if (cursor != null)
        {
            cursor.SetPlayer(currentPlayer);
        }
    }
}