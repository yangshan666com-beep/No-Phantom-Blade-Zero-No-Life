using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// One-click export of the current menu scene into a reusable UI prefab.
/// Menu items are ordered: run 1, then 2.
/// </summary>
public static class UIModuleExporter
{
    private const string PrefabFolder = "Assets/UI/Prefabs";
    private const string PrefabPath = PrefabFolder + "/MainMenuPanel.prefab";

    [MenuItem("Tools/UI Module/1 - Remove Pause UI from menu scene")]
    public static void RemovePauseUI()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[UIModuleExporter] No Canvas in the open scene.");
            return;
        }

        int removed = 0;
        removed += RemoveChild(canvas.transform, "PausePanel");
        removed += RemoveRoot("PauseManager");

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        EditorSceneManager.SaveOpenScenes();

        Debug.Log($"[UIModuleExporter] Removed {removed} pause object(s) from the menu scene.");
    }

    /// <summary>Batch-mode entry: opens the menu scene, cleans it and exports the prefab.</summary>
    public static void BatchExport()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/UIstart.unity", OpenSceneMode.Single);
        RemovePauseUI();
        CreatePrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[UIModuleExporter] Batch export done.");
    }

    [MenuItem("Tools/UI Module/2 - Create MainMenuPanel prefab")]
    public static void CreatePrefab()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[UIModuleExporter] No Canvas in the open scene.");
            return;
        }

        // 1) root
        var root = new GameObject("MainMenuPanel", typeof(RectTransform));
        var rt = (RectTransform)root.transform;
        rt.SetParent(canvas.transform, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // 2) move menu parts under the root
        MoveInto(root.transform, canvas.transform, "MainPanel");
        MoveInto(root.transform, canvas.transform, "settingPanel");
        MoveInto(root.transform, canvas.transform, "Manager");

        // 3) controller
        var controller = root.GetComponent<MainMenuController>();
        if (controller == null)
            controller = root.AddComponent<MainMenuController>();

        // 4) rewire the main buttons through the controller (UnityEvent only)
        RewireByName(root.transform, "startbutton", controller.StartNewGame);
        RewireByName(root.transform, "quitbutton", controller.QuitGame);

        // 5) ensure folders + save prefab
        if (!AssetDatabase.IsValidFolder("Assets/UI"))
            AssetDatabase.CreateFolder("Assets", "UI");
        if (!AssetDatabase.IsValidFolder(PrefabFolder))
            AssetDatabase.CreateFolder("Assets/UI", "Prefabs");

        PrefabUtility.SaveAsPrefabAssetAndConnect(root, PrefabPath, InteractionMode.AutomatedAction);

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[UIModuleExporter] MainMenuPanel.prefab created at " + PrefabPath);
    }

    // ================= helpers =================

    private static int RemoveChild(Transform parent, string name)
    {
        var t = parent.Find(name);
        if (t == null) return 0;
        Object.DestroyImmediate(t.gameObject);
        return 1;
    }

    private static int RemoveRoot(string name)
    {
        var go = GameObject.Find(name);
        if (go == null) return 0;
        Object.DestroyImmediate(go);
        return 1;
    }

    private static void MoveInto(Transform newParent, Transform canvas, string name)
    {
        var t = canvas.Find(name);
        if (t == null)
        {
            Debug.LogWarning("[UIModuleExporter] Missing object: " + name);
            return;
        }

        var world = t.position;
        t.SetParent(newParent, true);
        t.position = world;
    }

    private static void RewireByName(Transform root, string buttonName, UnityAction action)
    {
        var t = FindDeep(root, buttonName);
        if (t == null)
        {
            Debug.LogWarning("[UIModuleExporter] Button not found: " + buttonName);
            return;
        }

        var button = t.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning("[UIModuleExporter] No Button component on: " + buttonName);
            return;
        }

        var so = new SerializedObject(button);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        if (calls != null)
        {
            calls.arraySize = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        UnityEventTools.AddPersistentListener(button.onClick, action);
    }

    private static Transform FindDeep(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name)
                return t;
        return null;
    }
}