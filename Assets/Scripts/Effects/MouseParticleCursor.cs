using UnityEngine;

public class MouseParticleCursor : MonoBehaviour
{
    static MouseParticleCursor instance;

    SpriteRenderer circle;
    Texture2D circleTexture;
    Sprite circleSprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        Ensure();
    }

    static void Ensure()
    {
        if (FindObjectOfType<MouseParticleCursor>() != null)
        {
            return;
        }

        GameObject go = new GameObject("MouseParticleCursor");
        go.AddComponent<MouseParticleCursor>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Build();
    }

    void Build()
    {
        circleTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        circleTexture.name = "MouseWhiteCircle";
        circleTexture.filterMode = FilterMode.Bilinear;
        circleTexture.wrapMode = TextureWrapMode.Clamp;

        Color32[] pixels = new Color32[64 * 64];
        float center = 31.5f;
        float radius = 28f;

        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01((radius - dist) / 2f);
                pixels[y * 64 + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        circleTexture.SetPixels32(pixels);
        circleTexture.Apply();

        circleSprite = Sprite.Create(circleTexture, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f), 100f);
        circleSprite.name = "MouseWhiteCircleSprite";

        GameObject visual = new GameObject("Circle");
        visual.transform.SetParent(transform, false);
        circle = visual.AddComponent<SpriteRenderer>();
        circle.sprite = circleSprite;
        circle.color = Color.white;
        circle.sortingOrder = 6000;
        visual.transform.localScale = Vector3.one * 1.2f;
    }

    void LateUpdate()
    {
        if (circle == null)
        {
            return;
        }

        if (PadInput.Connected && PadInput.CursorMode && GravityCursor.Instance != null)
        {
            circle.enabled = true;
            Vector3 padCursor = GravityCursor.Instance.transform.position;
            padCursor.z = 0f;
            transform.position = padCursor;
            return;
        }

        Camera camera = Camera.main;

        if (camera == null)
        {
            circle.enabled = false;
            return;
        }

        Vector3 mouse = Input.mousePosition;
        bool inside = mouse.x >= 0f && mouse.x <= Screen.width && mouse.y >= 0f && mouse.y <= Screen.height;

        if (!inside)
        {
            circle.enabled = false;
            return;
        }

        circle.enabled = true;
        Vector3 world = camera.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, 0f));
        world.z = 0f;
        transform.position = world;
    }

    void OnDestroy()
    {
        if (circleSprite != null)
        {
            Destroy(circleSprite);
        }

        if (circleTexture != null)
        {
            Destroy(circleTexture);
        }

        if (instance == this)
        {
            instance = null;
        }
    }
}
