using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePlayer : MonoBehaviour
{ 
    [Header("主角预制体")]
    public GameObject playerPrefab;

    Player1 current;

    void OnEnable()  { GameSignals.LevelReady += HandleLevelReady; }
    void OnDisable() { GameSignals.LevelReady -= HandleLevelReady; }

    void HandleLevelReady(int levelIndex)
    {
        Respawn();
    }

    public void Respawn()
    {
        if (current != null)
        {
            Destroy(current.gameObject);
        }

        Vector3 pos = Vector3.zero;
        FuHuoDian sp = FindObjectOfType<FuHuoDian>();
        if (sp != null)
        {
            pos = sp.transform.position;
        }
        else
        {
            Debug.LogWarning("[PlayerSpawner] 关卡里没有 FuHuoDian,使用兜底坐标");
        }

        GameObject go = Instantiate(playerPrefab, pos, Quaternion.identity);
        go.name = "Player";

        current = go.GetComponent<Player1>();
        if (current == null)
        {
            Debug.LogError("[PlayerSpawner] playerPrefab 上没有挂 Player1");
            return;
        }

        current.SetControlEnabled(false);

        GameSignals.MarkPlayerSpawned(current);   // 通知所有人：玩家就位

        current.SetControlEnabled(true);          // 以后要加入场过场就挪到这里之后
    }
}
