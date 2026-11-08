using UnityEngine;
using UnityEngine.UI;

public class SkillFlag : MonoBehaviour
{
    [Header("技能图标")]
    public Sprite selfIcon;
    public Sprite singleIcon;
    public Sprite globalIcon;

    [Header("剩余次数上限")]
    public int selfMax = 3;
    public int singleMax = 3;
    public int globalMax = 1;

    static SkillFlag current;

    int selfLeft;
    int singleLeft;
    int globalLeft;
    Text[] countTexts = new Text[3];
    Image[] countIcons = new Image[3];
    Canvas canvas;
    GameObject hudRoot;

    void Awake()
    {
        if (current != null && current != this)
        {
            current.DestroyHud();
        }

        current = this;
        selfLeft = selfMax;
        singleLeft = singleMax;
        globalLeft = globalMax;
    }

    void Start()
    {
        BuildUI();
        Refresh();
    }

    void OnDestroy()
    {
        DestroyHud();

        if (current == this)
        {
            current = null;
        }
    }

    void DestroyHud()
    {
        if (hudRoot != null)
        {
            Destroy(hudRoot);
            hudRoot = null;
            canvas = null;
            countTexts[0] = null;
            countTexts[1] = null;
            countTexts[2] = null;
        }
    }

    public static bool TryUseSelf()
    {
        return current == null || current.Consume(0);
    }

    public static bool TryUseSingle()
    {
        return current == null || current.Consume(1);
    }

    public static bool TryUseGlobal()
    {
        return current == null || current.Consume(2);
    }

    public static void ResetCharges()
    {
        if (current == null)
        {
            return;
        }

        current.selfLeft = current.selfMax;
        current.singleLeft = current.singleMax;
        current.globalLeft = current.globalMax;
        current.Refresh();
    }

    bool Consume(int index)
    {
        if (index == 0)
        {
            if (selfLeft <= 0)
            {
                return false;
            }

            selfLeft--;
        }
        else if (index == 1)
        {
            if (singleLeft <= 0)
            {
                return false;
            }

            singleLeft--;
        }
        else
        {
            if (globalLeft <= 0)
            {
                return false;
            }

            globalLeft--;
        }

        Refresh();
        return true;
    }

    void Refresh()
    {
        if (countTexts[0] != null)
        {
            countTexts[0].text = selfLeft.ToString();
            countTexts[1].text = singleLeft.ToString();
            countTexts[2].text = globalLeft.ToString();
        }
    }

    void BuildUI()
    {
        if (hudRoot != null)
        {
            return;
        }

        hudRoot = new GameObject("SkillHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvas = hudRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = hudRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("SkillPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(hudRoot.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(0f, 0f);
        panelRect.pivot = new Vector2(0f, 0f);
        panelRect.anchoredPosition = new Vector2(28f, 28f);
        panelRect.sizeDelta = new Vector2(390f, 118f);
        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.035f, 0.05f, 0.09f, 0.82f);
        panelImage.raycastTarget = false;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        Sprite[] icons = { selfIcon, singleIcon, globalIcon };
        float[] xs = { 68f, 195f, 322f };

        for (int i = 0; i < 3; i++)
        {
            GameObject icon = new GameObject("Icon" + i, typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(panel.transform, false);
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0f);
            iconRect.anchorMax = new Vector2(0f, 0f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(xs[i], 86f);
            iconRect.sizeDelta = new Vector2(62f, 62f);
            Image image = icon.GetComponent<Image>();
            image.sprite = icons[i];
            image.preserveAspect = true;
            image.raycastTarget = false;

            GameObject label = new GameObject("Count" + i, typeof(RectTransform), typeof(Text));
            label.transform.SetParent(panel.transform, false);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = new Vector2(xs[i], 30f);
            labelRect.sizeDelta = new Vector2(86f, 34f);
            Text text = label.GetComponent<Text>();
            text.font = font;
            text.fontSize = 28;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            countTexts[i] = text;
        }
    }
}
