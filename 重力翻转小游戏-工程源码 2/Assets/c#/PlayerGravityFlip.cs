using UnityEngine;

public class PlayerGravityFlip : MonoBehaviour
{
    void Awake()
    {
        if (GetComponent<Player1>() != null) enabled = false;
    }
}
