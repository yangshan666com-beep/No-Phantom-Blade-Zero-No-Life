using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 把「关卡流程」接进当前场景：
///   1. 场景缺 EventSystem 就补一个
///   2. 找到（或创建）管理器物体，挂上 CreateLevel
///   3. 把 Assets/Level/level1~4.prefab 填进 CreateLevel.levels
///   4. 无需额外设置：CreateLevel.Start() 会读 LevelSelection.SelectedIndex 自动加载对应关卡
///   5. 检查每个关卡 prefab 里有没有 FuHuoDian（复活点），缺的自动补一个（放在地图底部中央）
///
/// 菜单：Tools / 接上关卡流程
/// 在 Main 场景里执行。
/// </summary>
public static class LevelFlowInstaller
{
    private static readonly string[] LevelPaths =
    {
        "Assets/Level/level1.prefab",
        "Assets/Level/level2.prefab",
        "Assets/Level/level3.prefab",
        "Assets/Level/level4.prefab",
    };

    [MenuItem("Tools/接上关卡流程 (Level Flow)")]
    public static void Install()
    {
        Scene scene = EditorSceneManager.GetActiveScene();

        if (string.IsNullOrEmpty(scene.path))
        {
            EditorUtility.DisplayDialog("关卡流程", "请先保存当前场景（Ctrl+S），再执行。", "好");
            return;
        }

        // ---------- 1. EventSystem ----------
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(es, "Add EventSystem");
            Debug.Log("[LevelFlowInstaller] 已补 EventSystem");
        }

        // ---------- 2. 管理器物体 ----------
        GameObject mgr = GameObject.Find("Jichu");
        if (mgr == null)
        {
            mgr = GameObject.Find("[Managers]");
        }
        if (mgr == null)
        {
            mgr = new GameObject("[Managers]");
            Undo.RegisterCreatedObjectUndo(mgr, "Add Managers");
            Debug.Log("[LevelFlowInstaller] 新建 [Managers] 物体");
        }

        CreateLevel creator = mgr.GetComponent<CreateLevel>();
        if (creator == null)
        {
            creator = mgr.AddComponent<CreateLevel>();
            Debug.Log("[LevelFlowInstaller] 已给 " + mgr.name + " 挂 CreateLevel");
        }
        else
        {
            Debug.Log("[LevelFlowInstaller] " + mgr.name + " 上已有 CreateLevel，复用");
        }

        // ---------- 3. 填 levels ----------
        List<GameObject> prefabs = new List<GameObject>();
        foreach (string path in LevelPaths)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset != null)
            {
                prefabs.Add(asset);
            }
            else
            {
                Debug.LogWarning("[LevelFlowInstaller] 找不到关卡预制体：" + path);
            }
        }

        SerializedObject so = new SerializedObject(creator);
        SerializedProperty levelsProp = so.FindProperty("levels");
        levelsProp.arraySize = prefabs.Count;

        for (int i = 0; i < prefabs.Count; i++)
        {
            SerializedProperty el = levelsProp.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("displayName").stringValue = prefabs[i].name;
            el.FindPropertyRelative("layoutPrefab").objectReferenceValue = prefabs[i];
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(creator);

        Debug.Log("[LevelFlowInstaller] 已配置 " + prefabs.Count + " 个关卡；CreateLevel.Start() 会按菜单选中的关卡自动加载");

        // ---------- 4. 确保每个关卡有复活点 ----------
        foreach (string path in LevelPaths)
        {
            EnsureSpawnPoint(path);
        }

        // ---------- 5. 收尾 ----------
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = mgr;

        Debug.Log("[LevelFlowInstaller] 完成。请 Ctrl+S 保存场景，然后 Play；也可以从 UIstart 点开始进入。");
    }

    /// <summary>关卡 prefab 里没有 FuHuoDian 就自动补一个（地图底部中央）</summary>
    private static void EnsureSpawnPoint(string path)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null)
        {
            return;
        }

        if (asset.GetComponentInChildren<FuHuoDian>(true) != null)
        {
            Debug.Log("[LevelFlowInstaller] " + asset.name + " 已有 FuHuoDian，跳过");
            return;
        }

        GameObject contents = PrefabUtility.LoadPrefabContents(path);

        try
        {
            Renderer[] renderers = contents.GetComponentsInChildren<Renderer>(true);
            Vector3 spawnPos;

            if (renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    b.Encapsulate(renderers[i].bounds);
                }
                spawnPos = new Vector3(b.center.x, b.min.y + 2f, 0f);
            }
            else
            {
                spawnPos = new Vector3(0f, 2f, 0f);
            }

            GameObject go = new GameObject("FuHuoDian");
            go.transform.SetParent(contents.transform, false);
            go.transform.position = spawnPos;
            go.AddComponent<FuHuoDian>();

            PrefabUtility.SaveAsPrefabAsset(contents, path);

            Debug.Log("[LevelFlowInstaller] " + asset.name + " 补了 FuHuoDian @ " + spawnPos
                      + "（如果主角掉出地图，请在预制体里手动调整这个位置）");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }
}
