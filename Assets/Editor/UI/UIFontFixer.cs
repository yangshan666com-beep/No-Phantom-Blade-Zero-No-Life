using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 一键把 UI 里丢失的字体换成可用字体。
/// 同时处理 MainMenuPanel.prefab 和【当前打开的场景】，因为场景里的空覆盖优先级更高。
/// 用法：菜单栏 Tools → UI → 修复字体引用
/// </summary>
public static class UIFontFixer
{
    const string TmpFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    static readonly string[] Prefabs =
    {
        "Assets/Prefabs/MainMenuPanel.prefab",
    };

    [MenuItem("Tools/UI/修复字体引用")]
    static void FixAll()
    {
        TMP_FontAsset tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
        Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (tmpFont == null)
        {
            Debug.LogError("[UIFontFixer] 找不到字体资产: " + TmpFontPath);
            return;
        }

        int total = 0;

        foreach (string path in Prefabs)
        {
            if (!System.IO.File.Exists(path))
            {
                Debug.LogWarning("[UIFontFixer] 预制体不存在，跳过: " + path);
                continue;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                int n = FixOn(root, tmpFont, legacyFont);
                total += n;
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log("[UIFontFixer] " + path + " 修复 " + n + " 个文字对象");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        Scene scene = SceneManager.GetActiveScene();
        if (scene.IsValid() && scene.isLoaded)
        {
            int n = 0;
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                n += FixOn(go, tmpFont, legacyFont);
            }

            if (n > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            total += n;
            Debug.Log("[UIFontFixer] 场景 " + scene.name + " 修复 " + n + " 个文字对象");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[UIFontFixer] 全部完成，共修复 " + total + " 个文字对象。记得 Ctrl+S 保存场景。");
    }

    static int FixOn(GameObject root, TMP_FontAsset tmpFont, Font legacyFont)
    {
        int n = 0;

        foreach (TMP_Text t in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (t.font != tmpFont)
            {
                t.font = tmpFont;
                n++;
            }
        }

        foreach (Text t in root.GetComponentsInChildren<Text>(true))
        {
            if (legacyFont != null && t.font != legacyFont)
            {
                t.font = legacyFont;
                n++;
            }
        }

        return n;
    }
}