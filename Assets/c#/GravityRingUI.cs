using UnityEngine;
using UnityEngine.SceneManagement;

public class GravityRingUI : MonoBehaviour
{
    public float holdTime = 0.1f;
    public float deadZone = 0.35f;
    public float radiusScale = 1.75f;
    public float activeGrow = 0.12f;
    public float directionTime = 0.2f;
    public float appearTime = 0.22f;
    public float disappearTime = 0.18f;
    public float tintSpeed = 6f;
    public int textureSize = 160;
    public Color idleColor = new Color(0.24f, 0.34f, 0.4f, 1f);
    public Color activeColor = new Color(0.56f, 0.82f, 1f, 1f);
    public Color tintColor = new Color(0.63f, 0.86f, 1f, 1f);

    public static bool PadActive { get; private set; }

    static readonly float[] centers = { 0f, 90f, 180f, -90f };

    GravityBlock block;
    Collider2D body;
    SpriteRenderer blockSprite;
    SpriteRenderer tintOverlay;
    Color blockColor;
    bool holding;
    bool activated;
    bool padMode;
    bool debugHold;
    float pressTime;
    float alpha;
    float tint;
    float[] segmentGrow = new float[4];
    float[] shownSegment = new float[4];
    Vector2 direction;
    bool hasDirection;
    bool stickWasActive;

    SpriteRenderer ringRenderer;
    Texture2D ringTexture;
    Color32[] ringBuffer;
    Vector3 ringCenter;
    float ringScale;
    int sortingLayer;
    float lastBuildTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void BootAfter()
    {
        Ensure();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Ensure();
    }

    static void Ensure()
    {
        if (FindObjectOfType<GravityRingUI>() != null)
        {
            return;
        }

        GameObject go = new GameObject("GravityRingUI");
        go.AddComponent<GravityRingUI>();
    }

    void OnDisable()
    {
        if (padMode)
        {
            RestoreTint();
        }

        padMode = false;
        PadActive = false;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!holding && (Input.GetKeyDown(KeyCode.F8) || Input.GetKeyDown(KeyCode.Alpha0)))
        {
            DebugBegin();
        }

        if (holding && activated)
        {
            if (Input.GetKeyDown(KeyCode.F9) || Input.GetKeyDown(KeyCode.Alpha1))
            {
                DebugSetDirection(Vector2.up);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                DebugSetDirection(Vector2.right);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                DebugSetDirection(Vector2.down);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                DebugSetDirection(Vector2.left);
            }
            if (Input.GetKeyDown(KeyCode.F10) || Input.GetKeyDown(KeyCode.Alpha9))
            {
                End();
            }
        }
#endif

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cancel();
            return;
        }

        if (PadInput.Connected)
        {
            UpdatePad();
            return;
        }

        if (!holding)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Begin();
            }

            return;
        }

        if (!activated)
        {
            if (Time.time - pressTime >= holdTime)
            {
                Activate();
            }

            if (!Input.GetMouseButton(0) && !debugHold)
            {
                End();
            }

            return;
        }

        UpdateDirection();

        if (!Input.GetMouseButton(0) && !debugHold)
        {
            End();
        }
    }

    void Begin()
    {
        if (Camera.main == null)
        {
            return;
        }

        Vector2 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GravityBlock found = FindBlock(world);

        if (found == null)
        {
            return;
        }

        Collider2D foundBody = found.GetComponentInChildren<Collider2D>();

        if (foundBody == null)
        {
            return;
        }

        StartHold(found, foundBody);
    }

    GravityBlock FindBlock(Vector2 world)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(world);
        int i = 0;

        while (i < hits.Length)
        {
            GravityBlock item = hits[i].GetComponentInParent<GravityBlock>();

            if (item != null)
            {
                return item;
            }

            i = i + 1;
        }

        GravityBlock[] blocks = FindObjectsOfType<GravityBlock>();
        GravityBlock best = null;
        float bestDist = 0.6f;
        i = 0;

        while (i < blocks.Length)
        {
            Collider2D itemBody = blocks[i].GetComponentInChildren<Collider2D>();

            if (itemBody != null)
            {
                Vector2 near = itemBody.ClosestPoint(world);
                float dist = Vector2.Distance(near, world);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = blocks[i];
                }
            }

            i = i + 1;
        }

        return best;
    }

    void StartHold(GravityBlock target, Collider2D targetBody)
    {
        block = target;
        body = targetBody;
        blockSprite = block.GetComponent<SpriteRenderer>();

        if (blockSprite == null)
        {
            ClearBlock();
            return;
        }

        blockColor = blockSprite.color;
        sortingLayer = blockSprite.sortingLayerID;
        holding = true;
        activated = false;
        hasDirection = false;
        stickWasActive = false;
        alpha = 0f;
        tint = 0f;
        pressTime = Time.time;
        ringCenter = body.bounds.center;
        ClearSegments();
        MakeTint();
    }

    void ClearSegments()
    {
        for (int i = 0; i < 4; i++)
        {
            segmentGrow[i] = 0f;
            shownSegment[i] = -1f;
        }
    }

    void Activate()
    {
        activated = true;
        BuildRing();
    }

    void UpdateDirection()
    {
        if (Camera.main == null || body == null)
        {
            return;
        }

        ringCenter = body.bounds.center;
        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 delta = mouse - (Vector2)ringCenter;

        if (delta.magnitude < deadZone)
        {
            hasDirection = false;
            return;
        }

        hasDirection = true;

        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        {
            direction = delta.x > 0f ? Vector2.right : Vector2.left;
        }
        else
        {
            direction = delta.y > 0f ? Vector2.up : Vector2.down;
        }
    }

    void UpdatePad()
    {
        if (!PadInput.CursorMode)
        {
            if (padMode)
            {
                FinishPad(true);
            }

            return;
        }

        GravityBlock target = GravityCursor.Instance != null ? GravityCursor.Instance.Target : null;

        if (target == null)
        {
            if (padMode)
            {
                FinishPad(false);
            }

            return;
        }

        Collider2D targetBody = target.GetComponentInChildren<Collider2D>();

        if (targetBody == null)
        {
            if (padMode)
            {
                FinishPad(false);
            }

            return;
        }

        if (!padMode || block != target)
        {
            if (padMode)
            {
                FinishPad(false);
            }

            StartPad(target, targetBody);
        }

        if (!activated)
        {
            Activate();
        }

        ReadPadDirection();
    }

    void StartPad(GravityBlock target, Collider2D targetBody)
    {
        StartHold(target, targetBody);
        padMode = true;
        PadActive = true;
        Activate();
    }

    void FinishPad(bool apply)
    {
        if (apply && hasDirection && block != null)
        {
            block.SetDirection(direction);
        }

        if (body != null)
        {
            ringCenter = body.bounds.center;
        }

        holding = false;
        activated = false;
        hasDirection = false;
        stickWasActive = false;
        padMode = false;
        PadActive = false;
        RestoreTint();
    }

    void ReadPadDirection()
    {
        Vector2 stick = PadInput.RightStick;
        bool pushed = stick.sqrMagnitude > 0.25f;

        if (pushed)
        {
            hasDirection = true;
            stickWasActive = true;

            if (Mathf.Abs(stick.x) >= Mathf.Abs(stick.y))
            {
                direction = stick.x > 0f ? Vector2.right : Vector2.left;
            }
            else
            {
                direction = stick.y > 0f ? Vector2.up : Vector2.down;
            }
        }
        else if (stickWasActive && stick.sqrMagnitude <= 0.0001f)
        {
            stickWasActive = false;

            if (hasDirection && block != null)
            {
                block.SetDirection(direction);
            }
        }

        if (PadInput.UpPressed)
        {
            direction = Vector2.up;
            hasDirection = true;
        }
        if (PadInput.DownPressed)
        {
            direction = Vector2.down;
            hasDirection = true;
        }
        if (PadInput.LeftPressed)
        {
            direction = Vector2.left;
            hasDirection = true;
        }
        if (PadInput.RightPressed)
        {
            direction = Vector2.right;
            hasDirection = true;
        }
    }

    void End()
    {
        if (activated && hasDirection && block != null)
        {
            block.SetDirection(direction);
        }

        if (body != null)
        {
            ringCenter = body.bounds.center;
        }

        holding = false;
        activated = false;
        hasDirection = false;
        debugHold = false;
        padMode = false;
        PadActive = false;
        stickWasActive = false;
        RestoreTint();
    }

    void Cancel()
    {
        holding = false;
        activated = false;
        hasDirection = false;
        debugHold = false;
        padMode = false;
        PadActive = false;
        stickWasActive = false;
        RestoreTint();
    }

    void MakeTint()
    {
        GameObject tintObject = new GameObject("RingTint");
        tintObject.transform.SetParent(blockSprite.transform, false);
        tintOverlay = tintObject.AddComponent<SpriteRenderer>();
        tintOverlay.sprite = blockSprite.sprite;
        tintOverlay.sortingOrder = blockSprite.sortingOrder + 1;
        tintOverlay.sortingLayerID = blockSprite.sortingLayerID;
        tintOverlay.color = new Color(tintColor.r, tintColor.g, tintColor.b, 0f);
    }

    void RestoreTint()
    {
        if (tintOverlay != null)
        {
            Destroy(tintOverlay.gameObject);
        }

        tintOverlay = null;

        if (blockSprite != null)
        {
            blockSprite.color = blockColor;
        }

        block = null;
        body = null;
        blockSprite = null;
    }

    void ClearBlock()
    {
        block = null;
        body = null;
        blockSprite = null;
        holding = false;
        activated = false;
        hasDirection = false;
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        float targetAlpha = (holding && activated) ? 1f : 0f;
        float duration = targetAlpha > alpha ? appearTime : disappearTime;

        if (duration < 0.01f)
        {
            duration = 0.01f;
        }

        alpha = Mathf.MoveTowards(alpha, targetAlpha, dt / duration);

        if (blockSprite != null)
        {
            float targetTint = holding ? 1f : 0f;
            tint = Mathf.MoveTowards(tint, targetTint, dt * tintSpeed);
            blockSprite.color = Color.Lerp(blockColor, tintColor, tint * 0.85f);

            if (tintOverlay != null)
            {
                Color overlay = tintOverlay.color;
                overlay.a = tint * 0.45f;
                tintOverlay.color = overlay;
            }

            if (body != null)
            {
                ringCenter = body.bounds.center;
            }
        }

        UpdateRing(dt);
    }

    void BuildRing()
    {
        if (ringRenderer == null)
        {
            GameObject go = new GameObject("RingSprite");
            go.transform.SetParent(transform, false);
            ringRenderer = go.AddComponent<SpriteRenderer>();
            ringRenderer.sortingOrder = 5000;
            ringRenderer.sortingLayerID = sortingLayer;

            ringTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            ringTexture.filterMode = FilterMode.Point;
            ringTexture.wrapMode = TextureWrapMode.Clamp;
            ringBuffer = new Color32[textureSize * textureSize];
            ringRenderer.sprite = Sprite.Create(ringTexture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
        }

        if (body == null)
        {
            return;
        }

        ringCenter = body.bounds.center;
        float ext = Mathf.Max(body.bounds.extents.x, body.bounds.extents.y);
        ringScale = ext * radiusScale / 0.38f;
        ringRenderer.transform.position = ringCenter;
        ringRenderer.transform.localScale = Vector3.one * ringScale;
        ClearSegments();
    }

    void UpdateRing(float dt)
    {
        if (ringRenderer == null)
        {
            return;
        }

        if (alpha <= 0.002f)
        {
            ringRenderer.enabled = false;
            return;
        }

        ringRenderer.enabled = true;
        ringRenderer.transform.position = ringCenter;

        float eased = alpha * alpha * (3f - 2f * alpha);
        float scaleMul;
        float rotation;

        if (activated)
        {
            scaleMul = Mathf.Lerp(0.82f, 1f, eased);
            rotation = Mathf.Lerp(-14f, 0f, eased);
        }
        else
        {
            scaleMul = 1f + 0.12f * (1f - eased);
            rotation = 12f * (1f - eased);
        }

        ringRenderer.transform.localScale = Vector3.one * (ringScale * scaleMul);
        ringRenderer.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

        int dir = hasDirection ? DirectionIndex(direction) : -1;
        float step = dt / Mathf.Max(0.01f, directionTime);
        bool dirty = false;
        int i = 0;

        while (i < 4)
        {
            float target = i == dir ? 1f : 0f;
            segmentGrow[i] = Mathf.MoveTowards(segmentGrow[i], target, step);

            if (Mathf.Abs(segmentGrow[i] - shownSegment[i]) > 0.004f)
            {
                dirty = true;
            }

            i = i + 1;
        }

        if (dirty && Time.time - lastBuildTime > 0.02f)
        {
            BuildRingTexture(segmentGrow);
            ringTexture.SetPixels32(ringBuffer);
            ringTexture.Apply(false);

            i = 0;

            while (i < 4)
            {
                shownSegment[i] = segmentGrow[i];
                i = i + 1;
            }

            lastBuildTime = Time.time;
        }

        Color color = ringRenderer.color;
        color.a = eased;
        ringRenderer.color = color;
    }

    int DirectionIndex(Vector2 dir)
    {
        if (dir == Vector2.right) return 0;
        if (dir == Vector2.up) return 1;
        if (dir == Vector2.left) return 2;
        return 3;
    }

    static Color32 Mix(Color32 a, Color32 b, float t)
    {
        t = Mathf.Clamp01(t);
        return new Color32(
            (byte)Mathf.RoundToInt(a.r + (b.r - a.r) * t),
            (byte)Mathf.RoundToInt(a.g + (b.g - a.g) * t),
            (byte)Mathf.RoundToInt(a.b + (b.b - a.b) * t),
            (byte)Mathf.RoundToInt(a.a + (b.a - a.a) * t));
    }

    void BuildRingTexture(float[] grows)
    {
        int S = textureSize;
        float half = S * 0.5f;
        float R = S * 0.38f;
        float innerBase = R * 0.82f;
        float halfSpan = 40f;
        Color32 dark = idleColor;
        Color32 bright = activeColor;
        Color32 darkRim = new Color32(105, 140, 160, 255);
        Color32 brightRim = new Color32(225, 245, 255, 255);
        Color32 darkShade = new Color32(40, 56, 68, 255);
        Color32 brightShade = new Color32(92, 150, 215, 255);

        int i = 0;

        while (i < ringBuffer.Length)
        {
            int x = i % S;
            int y = i / S;
            float dx = x + 0.5f - half;
            float dy = y + 0.5f - half;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float ang = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
            float bestG = -1f;
            Color32 band = new Color32(0, 0, 0, 0);
            byte glowA = 0;
            int d = 0;

            while (d < 4)
            {
                float g = Mathf.Clamp01(grows[d]);
                float grow = g * activeGrow * R;
                float outer = R + grow;
                float inner = innerBase - g * R * 0.05f;
                float span = halfSpan + 2f * g;
                float mid = (outer + inner) * 0.5f;
                float capR = (outer - inner) * 0.5f;
                float diff = Mathf.Abs(Mathf.DeltaAngle(ang, centers[d]));
                bool inside = diff <= span && dist <= outer && dist >= inner;

                if (!inside)
                {
                    for (int side = -1; side <= 1; side += 2)
                    {
                        float a = Mathf.Deg2Rad * (centers[d] + side * span);
                        float cxp = Mathf.Cos(a) * mid;
                        float cyp = Mathf.Sin(a) * mid;
                        float ddx = dx - cxp;
                        float ddy = dy - cyp;

                        if (ddx * ddx + ddy * ddy <= capR * capR)
                        {
                            inside = true;
                            break;
                        }
                    }
                }

                if (inside)
                {
                    if (g > bestG)
                    {
                        bestG = g;
                        float t = Mathf.InverseLerp(inner, outer, dist);
                        band = Mix(dark, bright, g);
                        Color32 rim = Mix(darkRim, brightRim, g);
                        Color32 shade = Mix(darkShade, brightShade, g);

                        if (t > 0.82f)
                        {
                            band = rim;
                        }
                        else if (t < 0.18f)
                        {
                            band = shade;
                        }

                        band.a = (byte)Mathf.RoundToInt(Mathf.Lerp(240f, 255f, g));
                    }
                }
                else if (g > 0.01f)
                {
                    float glowOuter = outer + R * 0.18f * g;

                    if (dist > outer && dist <= glowOuter && diff <= span + 8f)
                    {
                        float fade = 1f - Mathf.InverseLerp(outer, glowOuter, dist);
                        float angFade = 1f - Mathf.InverseLerp(span, span + 8f, diff);
                        byte a = (byte)Mathf.Clamp(fade * angFade * 150f * g, 0f, 255f);

                        if (a > glowA)
                        {
                            glowA = a;
                        }
                    }
                }

                d = d + 1;
            }

            if (bestG >= 0f)
            {
                ringBuffer[i] = band;
            }
            else if (glowA > 0)
            {
                ringBuffer[i] = new Color32(120, 200, 255, glowA);
            }
            else
            {
                ringBuffer[i] = new Color32(0, 0, 0, 0);
            }

            i = i + 1;
        }
    }

#if UNITY_EDITOR
    void DebugBegin()
    {
        GravityBlock[] blocks = FindObjectsOfType<GravityBlock>();
        Camera cam = Camera.main;

        if (blocks.Length == 0 || cam == null)
        {
            return;
        }

        GravityBlock best = null;
        float bestDist = float.MaxValue;
        int i = 0;

        while (i < blocks.Length)
        {
            Vector3 vp = cam.WorldToViewportPoint(blocks[i].transform.position);

            if (vp.z > 0f && vp.x > 0.05f && vp.x < 0.95f && vp.y > 0.05f && vp.y < 0.95f)
            {
                float d = Mathf.Abs(vp.x - 0.5f) + Mathf.Abs(vp.y - 0.5f);

                if (d < bestDist)
                {
                    bestDist = d;
                    best = blocks[i];
                }
            }

            i = i + 1;
        }

        if (best == null)
        {
            return;
        }

        StartHold(best, best.GetComponentInChildren<Collider2D>());
        debugHold = true;
        Activate();
        DebugSetDirection(Vector2.up);
    }

    void DebugSetDirection(Vector2 dir)
    {
        direction = dir;
        hasDirection = true;
        Debug.Log("GravityRingUI debug " + dir);
    }
#endif
}
