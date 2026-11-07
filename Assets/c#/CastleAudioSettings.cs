using UnityEngine;

[CreateAssetMenu(menuName = "城堡音频/设置", fileName = "CastleAudioSettings")]
public sealed class CastleAudioSettings : ScriptableObject
{
    [Header("A 版：沉闷靴跟与重型滑轨")]
    [Range(0f, 1f)] public float footstepVolume = 0.65f;
    [UnityEngine.Serialization.FormerlySerializedAs("gravityVolume")]
    [Range(0f, 1f)] public float mechanismStartVolume = 0.72f;
    [Range(0f, 1f)] public float landingVolume = 0.75f;
    [Range(0f, 1f)] public float musicVolume = 0.28f;
    [Header("物体运动与沉闷碰撞")]
    [Range(0f, 1f)] public float mechanismVolume = 0.5f;
    [Range(0f, 1f)] public float objectImpactVolume = 0.8f;
    [Min(0.1f)] public float minimumObjectImpactSpeed = 1.5f;
    [Min(0.05f)] public float objectImpactCooldown = 0.22f;
    [Min(0.05f)] public float arrivalStillTime = 0.18f;
    [Header("每一步的实际行进距离；走墙与天花板也适用")]
    [Min(0.1f)] public float strideDistance = 2.6f;
    [Min(0.05f)] public float minimumStepInterval = 0.17f;
}
