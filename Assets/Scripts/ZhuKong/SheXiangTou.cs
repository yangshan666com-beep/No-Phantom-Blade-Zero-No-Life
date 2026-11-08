using UnityEngine;
using Cinemachine;

/// <summary>
/// 2D 平台跳跃镜头：
/// · 上下：视野高度 = 关卡高度（整关一屏装下），垂直位置固定不动
/// · 左右：跟随玩家，带平滑
/// 挂到 Main 场景的 GameManager 上。
/// </summary>
[DefaultExecutionOrder(100)]
public class SheXiangTou : MonoBehaviour
{
    [Header("虚拟相机（留空自动找）")]
    public CinemachineVirtualCamera vcam;

    [Header("视野")]
    [Tooltip("关卡上下各留多少边距")]
    public float margin = 1.5f;

    [Tooltip("最小正交大小，防止小关卡放太大")]
    public float minOrthoSize = 5f;

    [Tooltip("是否连地图宽度也一起装下（默认关闭：只保证上下，左右靠跟随）")]
    public bool fitWidthToo = false;

    [Tooltip("宽高比，16:9 = 1.7778")]
    public float aspect = 16f / 9f;

    [Header("左右跟随")]
    [Tooltip("0 = 硬跟，越大越柔和")]
    public float smoothTime = 0.12f;

    Transform camTarget;
    Player1 player;
    float levelCenterY;
    float smoothVelocityX;

    void Awake()
    {
        if (vcam == null) vcam = GetComponent<CinemachineVirtualCamera>();
        if (vcam == null) vcam = FindObjectOfType<CinemachineVirtualCamera>();

        if (vcam == null)
        {
            Debug.LogError("[SheXiangTou] 找不到 CinemachineVirtualCamera，相机不会跟随");
            return;
        }

        // 建一个只动 X 的中间目标
        GameObject go = new GameObject("[CameraTarget]");
        camTarget = go.transform;
        camTarget.position = vcam.transform.position;

        vcam.Follow = camTarget;
        vcam.LookAt = null;
    }

    void OnEnable()
    {
        GameSignals.LevelReady += OnLevelReady;
        GameSignals.PlayerSpawned += OnPlayerSpawned;
    }

    void OnDisable()
    {
        GameSignals.LevelReady -= OnLevelReady;
        GameSignals.PlayerSpawned -= OnPlayerSpawned;
    }

    // 关卡生成完毕 → 量地图尺寸，算视野高度和垂直中心
    void OnLevelReady(int index)
    {
        GameObject root = GameObject.Find("[Level]");
        if (root == null || root.transform.childCount == 0)
        {
            Debug.LogWarning("[SheXiangTou] 找不到 [Level] 容器");
            return;
        }

        GameObject layout = root.transform.GetChild(0).gameObject;
        Renderer[] rs = layout.GetComponentsInChildren<Renderer>();

        if (rs.Length == 0)
        {
            Debug.LogWarning("[SheXiangTou] 关卡里没有 Renderer，无法计算视野");
            return;
        }

        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++)
        {
            b.Encapsulate(rs[i].bounds);
        }

        levelCenterY = b.center.y;

        float byHeight = b.extents.y + margin;
        float byWidth = (b.extents.x + margin) / aspect;
        float size = fitWidthToo ? Mathf.Max(byHeight, byWidth) : byHeight;
        size = Mathf.Max(size, minOrthoSize);

        vcam.m_Lens.Orthographic = true;
        vcam.m_Lens.OrthographicSize = size;

        if (camTarget != null)
        {
            camTarget.position = new Vector3(camTarget.position.x, levelCenterY, 0f);
        }

        Debug.Log("[SheXiangTou] 关卡 宽" + b.size.x.ToString("F1") + " 高" + b.size.y.ToString("F1")
                + " → 视野高度 " + (size * 2f).ToString("F1") + "，垂直中心 Y=" + levelCenterY.ToString("F1"));
    }

    void OnPlayerSpawned(Player1 p)
    {
        player = p;

        if (camTarget != null && p != null)
        {
            camTarget.position = new Vector3(p.transform.position.x, levelCenterY, 0f);
            smoothVelocityX = 0f;
        }
    }

    void LateUpdate()
    {
        if (player == null || camTarget == null)
        {
            return;
        }

        float targetX = player.transform.position.x;
        float newX = smoothTime <= 0.0001f
            ? targetX
            : Mathf.SmoothDamp(camTarget.position.x, targetX, ref smoothVelocityX, smoothTime);

        // Y 永远锁在 levelCenterY → 垂直方向不动
        camTarget.position = new Vector3(newX, levelCenterY, 0f);
    }
}