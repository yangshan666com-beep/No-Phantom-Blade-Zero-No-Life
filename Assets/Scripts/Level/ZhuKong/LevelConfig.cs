using System;
using UnityEngine;

/// 一个关卡的全部"规定"。多关卡 = 多建几个 asset，不改代码。
/// 创建方式：Project 右键 → Create → Game → Level Config
[CreateAssetMenu(menuName = "Game/Level Config", fileName = "Level_01")]
public class LevelConfig : ScriptableObject
{
    [Serializable]
    public class BlockSpec
    {
        public Vector2 position;
        public Vector2 gravityDirection = Vector2.down;
        [Range(0.1f, 5f)] public float scale = 1f;
    }

    [Header("基本信息")]
    public string displayName = "第一关";

    [Header("出生点")]
    public Vector2 playerSpawn = new Vector2(0f, -2f);

    [Header("地图范围（背景按这个尺寸铺）")]
    public Vector2 mapMin = new Vector2(-8f, -4.5f);
    public Vector2 mapMax = new Vector2(8f, 4.5f);

    [Header("预制体")]
    public GameObject playerPrefab;
    public GameObject blockPrefab;
    public GameObject backgroundPrefab;

    [Header("按规定摆放的方块")]
    public BlockSpec[] blocks = new BlockSpec[0];
}