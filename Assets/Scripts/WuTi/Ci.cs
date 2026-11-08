using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider2D))]

public class Ci : MonoBehaviour
{
    [Header("触发间隔")]
    public float coolDown = 0.5f;

    float lastHitTime = -999f;

    void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other) { TryKill(other); }
    void OnTriggerStay2D(Collider2D other)  { TryKill(other); }

    void TryKill(Collider2D other)
    {
        if (other == null) return;

        Player1 player = other.GetComponent<Player1>();
        if (player == null) player = other.GetComponentInParent<Player1>();
        if (player == null) return;

        if (Time.time - lastHitTime < coolDown) return;
        lastHitTime = Time.time;
        // 防止"重生瞬间又死"导致无限重建
   
        Debug.Log("[Ci] 玩家死亡 ");
        player.RespawnAtSpawn();
    }
}
