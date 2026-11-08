using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 一键把「暂停界面」接进当前场景：
///   1. 场景缺 EventSystem 就补一个（否则按钮点不动）
///   2. 给 PauseCanva.prefab 资产本身补 CanvasGroup（永久生效，不是场景覆盖）
///   3. 实例化 Assets/UI/Pause/Prefabs/PauseCanva.prefab 到场景
///   4. 建 PauseManager 挂 PauseMenu，填好 pausePanel / pauseGroup / mainMenuSceneName
///   5. 绑三个按钮：Pause → Resume，Return → Resume，Setting → OpenSettings
///   6. PauseCanva 初始隐藏
///
/// 菜单：Tools / 添加暂停界面
/// </summary>
public static class PauseUiInstaller
{
    private const string PrefabPath = "Assets/UI/Pause/Prefabs/PauseCanva.prefab";

    [MenuItem("Tools/添加暂停界面 (Pause UI)")]
    public static void Install()
    {
        Scene scene = EditorSceneManager.GetActiveScene();

        if (string.IsNullOrEmpty(scene.path))
        {
            EditorUtility.DisplayDialog("暂停界面", "请先保存当前场景（Ctrl+S），再执行。", "好");
            return;
        }

        if (scene.name == "UIstart")
        {
            bool go = EditorUtility.DisplayDialog(
                "暂停界面",
                "当前是 UIstart（主菜单）场景。\n暂停界面一般放在游戏场景（Main）。\n\n仍要继续吗？",
                "继续", "取消");
            if (!go)
            {
                return;
            }
        }

        // ---------- 1. EventSystem ----------
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(es, "Add EventSystem");
            Debug.Log("[PauseUiInstaller] 已补 EventSystem（没有它按钮点不动）");
        }
        else
        {
            Debug.Log("[PauseUiInstaller] 场景已有 EventSystem，跳过");
        }

        // ---------- 2. 预制体资产本身加 CanvasGroup ----------
        EnsurePrefabCanvasGroup();

        // ---------- 3. 实例化 / 复用 PauseCanva ----------
        GameObject canvaRoot = GameObject.Find("PauseCanva");
        if (canvaRoot == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("暂停界面", "找不到预制体：\n" + PrefabPath, "好");
                return;
            }
            canvaRoot = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(canvaRoot, "Add PauseCanva");
            canvaRoot.name = "PauseCanva";
            Debug.Log("[PauseUiInstaller] 已实例化 PauseCanva");
        }
        else
        {
            Debug.Log("[PauseUiInstaller] 场景里已有 PauseCanva，复用");
        }

        CanvasGroup group = canvaRoot.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = canvaRoot.AddComponent<CanvasGroup>();
            Debug.Log("[PauseUiInstaller] PauseCanva 补加 CanvasGroup");
        }

        // ---------- 4. PauseManager + PauseMenu ----------
        GameObject managerGo = GameObject.Find("PauseManager");
        if (managerGo == null)
        {
            managerGo = new GameObject("PauseManager");
            Undo.RegisterCreatedObjectUndo(managerGo, "Add PauseManager");
        }

        PauseMenu menu = managerGo.GetComponent<PauseMenu>();
        if (menu == null)
        {
            menu = managerGo.AddComponent<PauseMenu>();
            Debug.Log("[PauseUiInstaller] 已给 PauseManager 挂 PauseMenu");
        }

        // ---------- 5. 填私有序列化字段 ----------
        SerializedObject so = new SerializedObject(menu);
        SerializedProperty panelProp = so.FindProperty("pausePanel");
        SerializedProperty groupProp = so.FindProperty("pauseGroup");
        SerializedProperty sceneNameProp = so.FindProperty("mainMenuSceneName");

        if (panelProp != null) panelProp.objectReferenceValue = canvaRoot;
        if (groupProp != null) groupProp.objectReferenceValue = group;
        if (sceneNameProp != null) sceneNameProp.stringValue = "UIstart";
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(menu);
        Debug.Log("[PauseUiInstaller] pausePanel / pauseGroup / mainMenuSceneName 已填好");

        // ---------- 6. 绑按钮 ----------
        Bind(canvaRoot.transform, "Pause", menu, "Resume");
        Bind(canvaRoot.transform, "Return", menu, "Resume");
        Bind(canvaRoot.transform, "Setting", menu, "OpenSettings");

        // ---------- 7. 初始隐藏 ----------
        canvaRoot.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = managerGo;

        Debug.Log("[PauseUiInstaller] 全部完成。请按 Ctrl+S 保存场景，然后 Play，按 Esc 测试暂停。");
    }

    /// <summary>给预制体资产本身加 CanvasGroup（影响所有实例，不产生场景覆盖）</summary>
    private static void EnsurePrefabCanvasGroup()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
        {
            return;
        }

        GameObject contents = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            if (contents.GetComponent<CanvasGroup>() == null)
            {
                contents.AddComponent<CanvasGroup>();
                PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath);
                Debug.Log("[PauseUiInstaller] PauseCanva.prefab 已补 CanvasGroup（写入资产）");
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    private static void Bind(Transform root, string buttonName, PauseMenu target, string methodName)
    {
        Transform t = FindDeep(root, buttonName);
        if (t == null)
        {
            Debug.LogWarning("[PauseUiInstaller] 找不到按钮节点：" + buttonName);
            return;
        }

        Button btn = t.GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogWarning("[PauseUiInstaller] " + buttonName + " 上没有 Button 组件");
            return;
        }

        UnityAction action = System.Delegate.CreateDelegate(typeof(UnityAction), target, methodName) as UnityAction;
        if (action == null)
        {
            Debug.LogWarning("[PauseUiInstaller] 取不到方法：PauseMenu." + methodName);
            return;
        }

        btn.onClick.RemoveAllListeners();
        UnityEventTools.AddPersistentListener(btn.onClick, action);
        EditorUtility.SetDirty(btn);
        Debug.Log("[PauseUiInstaller] 按钮 " + buttonName + " -> PauseMenu." + methodName);
    }

    private static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform hit = FindDeep(root.GetChild(i), name);
            if (hit != null)
            {
                return hit;
            }
        }

        return null;
    }
}
