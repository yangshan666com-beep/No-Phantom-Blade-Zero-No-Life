using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LoadPlayerFromPrefab
{
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/MergedMap.unity");

        GameObject level1 = Find("level1");

        if (level1 == null)
        {
            Debug.LogError("LOAD PLAYER: 找不到 level1");
            return;
        }

        BoxCollider2D best = null;
        float bestArea = 0f;
        BoxCollider2D[] colliders = level1.GetComponentsInChildren<BoxCollider2D>(true);

        for (int i = 0; i < colliders.Length; i++)
        {
            Bounds bounds = colliders[i].bounds;
            bool horizontal = bounds.size.x >= bounds.size.y;
            float area = bounds.size.x * bounds.size.y;

            if (horizontal && area > bestArea)
            {
                bestArea = area;
                best = colliders[i];
            }
        }

        Vector3 spawn = Vector3.zero;

        if (best != null)
        {
            spawn = new Vector3(best.bounds.center.x, best.bounds.max.y + 1.2f, 0f);
        }
        else if (colliders.Length > 0)
        {
            Bounds bounds = colliders[0].bounds;
            spawn = new Vector3(bounds.center.x, bounds.max.y + 1.2f, 0f);
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");

        if (prefab == null)
        {
            Debug.LogError("LOAD PLAYER: Player.prefab 不存在");
            return;
        }

        Player1[] players = Object.FindObjectsOfType<Player1>(true);

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].gameObject.activeInHierarchy)
            {
                Object.DestroyImmediate(players[i].gameObject);
            }
        }

        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        player.name = "Player";
        player.transform.position = spawn;
        player.transform.rotation = Quaternion.identity;

        GravityCursor cursor = Object.FindObjectOfType<GravityCursor>(true);

        if (cursor != null)
        {
            cursor.SetPlayer(player.GetComponent<Player1>());
        }

        BindCamera(player);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("LOAD PLAYER DONE at " + spawn);
    }

    static void BindCamera(GameObject player)
    {
        MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>(true);
        int i = 0;

        while (i < behaviours.Length)
        {
            MonoBehaviour behaviour = behaviours[i];

            if (behaviour != null && behaviour.GetType().Name == "CinemachineVirtualCamera")
            {
                System.Type type = behaviour.GetType();
                PropertyInfo follow = type.GetProperty("Follow");
                PropertyInfo lookAt = type.GetProperty("LookAt");

                if (follow != null) follow.SetValue(behaviour, player.transform);
                if (lookAt != null) lookAt.SetValue(behaviour, player.transform);

                Debug.Log("LOAD PLAYER CAMERA BOUND");
                return;
            }

            i = i + 1;
        }
    }

    static GameObject Find(string name)
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] all = roots[i].GetComponentsInChildren<Transform>(true);

            for (int t = 0; t < all.Length; t++)
            {
                if (all[t].name == name)
                {
                    return all[t].gameObject;
                }
            }
        }

        return null;
    }
}
