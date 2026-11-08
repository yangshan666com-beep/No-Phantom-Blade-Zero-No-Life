using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZhongDian : MonoBehaviour
{

    [Header("通关时显示的界面（可留空）")]
    public GameObject winPanel;

    bool triggered = false;

    void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryFinish(other);
    }

    void TryFinish(Collider2D other)
    {
        if (triggered) { return; }
        triggered = true;
        // 只生效一次（玩家身上可能有多个碰撞体）

        if (other == null) { return; }

        Player1 player = other.GetComponentInParent<Player1>();
        if (player == null) { return; }

        CreateLevel level = FindObjectOfType<CreateLevel>();
        if (level == null)
        {
            Debug.LogError("[LevelGoal] 场景里没有挂 CreateLevel");
            return;
        }

        int next = level.CurrentLevelIndex + 1;

        // 已经超过最后一关 → 通关
        if (next >= level.levels.Count)
        {
            
            Debug.Log("[LevelGoal] 通关！共 " + level.levels.Count + " 关");

            player.SetControlEnabled(false);
            Cursor.visible = true;                  // 绿圈把鼠标藏了，通关时要还回来

            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }
            return;
        }

        Debug.Log("[LevelGoal] 第 " + (level.CurrentLevelIndex + 1) + " 关完成 → 进入第 " + (next + 1) + " 关");
        GameSignals.RequestLevel(next);
    }
}
