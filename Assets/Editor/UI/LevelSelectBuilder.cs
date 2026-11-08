using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Adds a LevelSelectPanel inside MainMenuPanel.prefab and wires:
///   Start -> fade out MainPanel / fade in LevelSelectPanel
///   Level buttons -> placeholder SelectLevel(index)
///   Back -> fade back to MainPanel
/// Level buttons turn gray while pressed (ButtonJuice pressedTint).
/// </summary>
public static class LevelSelectBuilder
{
    private const string PrefabPath = "Assets/UI/Prefabs/MainMenuPanel.prefab";
    private const int LevelCount = 6;

    [MenuItem("Tools/UI Module/3 - Add LevelSelectPanel")]
    public static void Build()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (asset == null)
        {
            Debug.LogError("[LevelSelectBuilder] Prefab not found: " + PrefabPath);
            return;
        }

        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            var mainPanel = root.transform.Find("MainPanel");
            if (mainPanel == null)
            {
                Debug.LogError("[LevelSelectBuilder] MainPanel not found inside the prefab.");
                return;
            }

            var mainGroup = mainPanel.GetComponent<CanvasGroup>();
            if (mainGroup == null)
                mainGroup = mainPanel.gameObject.AddComponent<CanvasGroup>();
            mainGroup.alpha = 1f;
            mainGroup.blocksRaycasts = true;
            mainGroup.interactable = true;

            // rebuild if exists
            var old = root.transform.Find("LevelSelectPanel");
            if (old != null)
                Object.DestroyImmediate(old.gameObject);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // panel
            var panel = new GameObject("LevelSelectPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var prt = (RectTransform)panel.transform;
            prt.SetParent(root.transform, false);
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.one;
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;

            var bg = panel.GetComponent<Image>();
            bg.color = new Color(0.039f, 0.055f, 0.078f, 0.92f);
            bg.raycastTarget = true;

            var group = panel.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            CreateText(panel.transform, "Title", "SELECT LEVEL", font, 48,
                new Vector2(0f, 220f), new Vector2(800f, 80f), Color.white);

            var buttons = new Button[LevelCount];
            for (int i = 0; i < LevelCount; i++)
            {
                int col = i % 3;
                int row = i / 3;
                var pos = new Vector2(-320f + col * 320f, 80f - row * 110f);

                var b = CreateButton(panel.transform, "LevelButton_" + (i + 1), (i + 1).ToString(),
                    font, sprite, pos, new Vector2(260f, 70f));

                MakePressGray(b);
                buttons[i] = b;
            }

            var back = CreateButton(panel.transform, "BackButton", "BACK", font, sprite,
                new Vector2(0f, -180f), new Vector2(220f, 60f));
            MakePressGray(back);

            // controller
            var controller = root.GetComponent<LevelSelectController>();
            if (controller == null)
                controller = root.AddComponent<LevelSelectController>();

            var so = new SerializedObject(controller);
            SetRef(so, "mainGroup", mainGroup);
            SetRef(so, "levelPanel", panel);
            SetRef(so, "levelGroup", group);
            so.ApplyModifiedPropertiesWithoutUndo();

            // wiring
            var start = FindDeep(root.transform, "startbutton");
            if (start != null && start.GetComponent<Button>() != null)
                Rewire(start.GetComponent<Button>(), controller.OpenLevelSelect);
            else
                Debug.LogWarning("[LevelSelectBuilder] startbutton not found.");

            for (int i = 0; i < LevelCount; i++)
            {
                UnityEventTools.AddIntPersistentListener(buttons[i].onClick, controller.SelectLevel, i);
            }

            UnityEventTools.AddPersistentListener(back.onClick, controller.CloseLevelSelect);

            panel.SetActive(false);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Debug.Log("[LevelSelectBuilder] LevelSelectPanel added to MainMenuPanel.prefab.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // ================= helpers =================

    private static void MakePressGray(Button b)
    {
        var juice = b.GetComponent<ButtonJuice>();
        if (juice == null)
            juice = b.gameObject.AddComponent<ButtonJuice>();

        var so = new SerializedObject(juice);
        var tint = so.FindProperty("pressedTint");
        if (tint != null)
        {
            tint.colorValue = new Color(0.45f, 0.45f, 0.45f, 1f);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetRef(SerializedObject so, string prop, Object value)
    {
        var p = so.FindProperty(prop);
        if (p != null)
            p.objectReferenceValue = value;
    }

    private static void Rewire(Button button, UnityAction action)
    {
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

    private static Text CreateText(Transform parent, string name, string content, Font font, int size,
        Vector2 pos, Vector2 rectSize, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = rectSize;

        var t = go.GetComponent<Text>();
        t.text = content;
        t.font = font;
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;

        return t;
    }

    private static Button CreateButton(Transform parent, string name, string label, Font font,
        Sprite sprite, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = new Color(0.85f, 0.85f, 0.85f, 1f);

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.None;

        var text = CreateText(go.transform, "Text", label, font, 30, Vector2.zero, size,
            new Color(0.055f, 0.086f, 0.129f, 1f));

        text.raycastTarget = false;

        return btn;
    }
}
