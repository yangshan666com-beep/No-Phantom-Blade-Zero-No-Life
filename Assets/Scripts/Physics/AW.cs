using UnityEngine;

public static class Physics2DSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Physics2D.IgnoreLayerCollision(30, 31, true);
    }
}