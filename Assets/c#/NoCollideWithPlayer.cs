using UnityEngine;

public class NoCollideWithPlayer : MonoBehaviour
{
    Player1 player;

    void Start()
    {
        player = FindObjectOfType<Player1>();

        if (player == null)
        {
            return;
        }

        Collider2D[] myColliders = GetComponentsInChildren<Collider2D>();
        Collider2D[] playerColliders = player.GetComponentsInChildren<Collider2D>();

        int i = 0;

        while (i < myColliders.Length)
        {
            int j = 0;

            while (j < playerColliders.Length)
            {
                Physics2D.IgnoreCollision(myColliders[i], playerColliders[j], true);
                j = j + 1;
            }

            i = i + 1;
        }
    }
}
