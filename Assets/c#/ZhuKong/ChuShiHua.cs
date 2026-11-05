using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuShiHua : MonoBehaviour
{
    [Header("帧率")]
    [Tooltip("0 = 平台默认；-1 = 极限;120 = 固定上限。")]
    public int targetFrameRate = 120;
    [Header("更新频率")]
    [Tooltip("默认 0.02 = 每秒 50 次 FixedUpdate。")]
    public float fixedDeltaTime = 0.02f;
    // Start is called before the first frame update
    public int GuanQia = 0;
    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Time.fixedDeltaTime = fixedDeltaTime;
        Application.targetFrameRate = targetFrameRate;
    }
    void Start()
    {
        //后续在此增加音乐触发代码
        Enter(GuanQia);       //暂时直接进入关卡1
    }
    public void Enter(int GuanQia)
    {
        Debug.Log("[Bootstrap] 请求进入第 " + GuanQia + " 关");
        GameSignals.RequestLevel(GuanQia);
    }
    
    // Update is called once per frame
}
