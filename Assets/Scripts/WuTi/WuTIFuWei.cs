using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(GravityBlock))]

public class WuTIFuWei : MonoBehaviour
{
   GravityBlock block;
    Rigidbody2D body;

    Vector3 startPos;
    Quaternion startRot;
    Vector2 startDir;

    void Awake()
    {
        block = GetComponent<GravityBlock>();
        body  = GetComponent<Rigidbody2D>();

        startPos = transform.position;
        startRot = transform.rotation;
        startDir = block.initialDirection;
    }
    void OnEnable()  { GameSignals.LevelReady += OnLevelReady; }
    void OnDisable() { GameSignals.LevelReady -= OnLevelReady; }
    void OnLevelReady(int index)
    {
        ResetToStart();
    }

    public void ResetToStart()
    {
        transform.position = startPos;
        transform.rotation = startRot;
        block.SetDirection(startDir);
        block.Select(false);          // 取消选中高亮，颜色恢复正常

        if (body != null)
        {
            body.velocity = Vector2.zero;       // 清掉速度
            body.angularVelocity = 0f;          // 清掉旋转
        }
    }
}
