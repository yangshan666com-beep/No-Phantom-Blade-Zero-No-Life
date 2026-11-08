using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RebindLevel1
{
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/MergedMap.unity");
        GameObject target = null;
        GameObject[] roots = scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] all = roots[i].GetComponentsInChildren<Transform>(true);

            for (int t = 0; t < all.Length; t++)
            {
                if (all[t].name == "level1")
                {
                    target = all[t].gameObject;
                    break;
                }
            }

            if (target != null)
            {
                break;
            }
        }

        if (target == null)
        {
            for (int i = 0; i < roots.Length; i++)
            {
                if (PrefabUtility.IsPrefabAssetMissing(roots[i]))
                {
                    target = roots[i];
                    break;
                }
            }
        }

        if (target == null)
        {
            Debug.LogError("REBIND LEVEL1: 找不到原来的 level1 实例");
            return;
        }

        Vector3 position = target.transform.position;
        Quaternion rotation = target.transform.rotation;
        Vector3 scale = target.transform.localScale;
        Transform parent = target.transform.parent;
        int sibling = target.transform.GetSiblingIndex();
        string oldName = target.name;

        Object.DestroyImmediate(target);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Level/level1.prefab");

        if (prefab == null)
        {
            Debug.LogError("REBIND LEVEL1: 新 level1.prefab 不存在");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        instance.name = oldName;
        instance.transform.SetParent(parent, true);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.localScale = scale;
        instance.transform.SetSiblingIndex(sibling);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        int missing = 0;

        for (int i = 0; i < scene.GetRootGameObjects().Length; i++)
        {
            if (PrefabUtility.IsPrefabAssetMissing(scene.GetRootGameObjects()[i]))
            {
                missing++;
            }
        }

        Debug.Log("REBIND LEVEL1 DONE missing=" + missing);
    }
}
