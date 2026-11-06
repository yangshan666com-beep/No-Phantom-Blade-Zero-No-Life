using System.Reflection;
using UnityEngine;

public class ZhuKongCameraBinder : MonoBehaviour
{
    public MonoBehaviour virtualCamera;

    void OnEnable()
    {
        GameSignals.LevelReady += OnLevelReady;
    }

    void OnDisable()
    {
        GameSignals.LevelReady -= OnLevelReady;
    }

    void OnLevelReady(int index)
    {
        if (virtualCamera == null)
        {
            return;
        }

        Player1 player = Object.FindObjectOfType<Player1>();

        if (player == null)
        {
            return;
        }

        System.Type type = virtualCamera.GetType();
        PropertyInfo follow = type.GetProperty("Follow");
        PropertyInfo lookAt = type.GetProperty("LookAt");

        if (follow != null)
        {
            follow.SetValue(virtualCamera, player.transform);
        }

        if (lookAt != null)
        {
            lookAt.SetValue(virtualCamera, player.transform);
        }

        Debug.Log("CAMERA BOUND: " + player.name + " -> " + type.Name);
    }
}
