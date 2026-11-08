using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Juicy press feedback for uGUI Button: squash-scale, tint flash, click SFX, haptics.
/// Attach to any GameObject that has a Button. No DOTween dependency.
/// </summary>
[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class ButtonJuice : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
{
    [Header("Scale")]
    [Tooltip("Uniform scale while pressed")]
    [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.92f;

    [Tooltip("Squash ratio: X widens, Y flattens while pressed")]
    [SerializeField, Range(0f, 0.3f)] private float squash = 0.08f;

    [Tooltip("Press-in duration (seconds), fast-in")]
    [SerializeField] private float pressDuration = 0.08f;

    [Tooltip("Release duration (seconds), slow-out with overshoot")]
    [SerializeField] private float releaseDuration = 0.15f;

    [Tooltip("Back-out overshoot strength, 1.7 = subtle bounce")]
    [SerializeField, Range(0f, 5f)] private float overshoot = 1.7f;

    [Header("Color")]
    [Tooltip("Leave empty to use Button.targetGraphic")]
    [SerializeField] private Image targetImage;

    [Tooltip("Multiplied with the button's base color while pressed")]
    [SerializeField] private Color pressedTint = new Color(0.82f, 0.82f, 0.82f, 1f);

    [Tooltip("Tint duration (seconds)")]
    [SerializeField] private float colorDuration = 0.08f;

    [Header("Audio")]
    [Tooltip("Leave empty to use a shared UI audio source")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip clickClip;

    [SerializeField, Range(0f, 1f)] private float clickVolume = 0.8f;

    [Header("Haptics")]
    [Tooltip("Mobile only; ignored on desktop")]
    [SerializeField] private bool enableHaptics = true;

    private Button button;
    private RectTransform rect;
    private Vector3 baseScale;
    private Color baseColor;
    private Coroutine scaleRoutine;
    private Coroutine colorRoutine;
    private bool isPressed;
    private bool pointerInside;

    private static AudioSource sharedSource;

    // ================= lifecycle =================

    void Awake()
    {
        button = GetComponent<Button>();
        rect = (RectTransform)transform;
        baseScale = rect.localScale;

        if (targetImage == null)
            targetImage = button.targetGraphic as Image;

        if (targetImage != null)
            baseColor = targetImage.color;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        pointerInside = false;
    }

    void OnDisable()
    {
        // Reset hard: panel hidden / scene unloaded mid-press must not leave button squashed
        StopAllCoroutines();
        scaleRoutine = null;
        colorRoutine = null;
        isPressed = false;
        pointerInside = false;

        if (rect != null)
            rect.localScale = baseScale;

        if (targetImage != null)
            targetImage.color = baseColor;
    }

    // ================= pointer =================

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button == null || !button.IsInteractable())
            return;

        pointerInside = true;
        isPressed = true;

        StartScale(PressedFactor(), pressDuration, false);
        StartColor(PressedColor(), colorDuration);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed)
            return;

        isPressed = false;

        StartScale(Vector3.one, releaseDuration, true);
        StartColor(baseColor, colorDuration);

        // Released outside the button: reset only, no click
        if (!pointerInside)
            return;

        PlayClick();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;

        if (!isPressed)
            return;

        // Dragged out while held: snap back immediately so it never sticks
        isPressed = false;
        StartScale(Vector3.one, pressDuration, false);
        StartColor(baseColor, colorDuration);
    }

    // ================= animation =================

    private Vector3 PressedFactor()
    {
        float sx = pressedScale * (1f + squash * 0.5f);
        float sy = pressedScale * (1f - squash * 0.5f);
        return new Vector3(sx, sy, 1f);
    }

    private Color PressedColor()
    {
        return new Color(
            baseColor.r * pressedTint.r,
            baseColor.g * pressedTint.g,
            baseColor.b * pressedTint.b,
            baseColor.a);
    }

    private void StartScale(Vector3 factor, float duration, bool withOvershoot)
    {
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(ScaleRoutine(factor, duration, withOvershoot));
    }

    private IEnumerator ScaleRoutine(Vector3 factor, float duration, bool withOvershoot)
    {
        Vector3 from = new Vector3(
            baseScale.x > 0.0001f ? rect.localScale.x / baseScale.x : 1f,
            baseScale.y > 0.0001f ? rect.localScale.y / baseScale.y : 1f,
            1f);

        float d = Mathf.Max(0.0001f, duration);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / d;
            float c = Mathf.Clamp01(t);
            float e = withOvershoot ? BackOut(c, overshoot) : EaseOutQuad(c);

            rect.localScale = new Vector3(
                baseScale.x * Mathf.LerpUnclamped(from.x, factor.x, e),
                baseScale.y * Mathf.LerpUnclamped(from.y, factor.y, e),
                baseScale.z);

            yield return null;
        }

        rect.localScale = new Vector3(baseScale.x * factor.x, baseScale.y * factor.y, baseScale.z);
        scaleRoutine = null;
    }

    private void StartColor(Color target, float duration)
    {
        if (targetImage == null)
            return;

        if (colorRoutine != null)
            StopCoroutine(colorRoutine);

        colorRoutine = StartCoroutine(ColorRoutine(target, duration));
    }

    private IEnumerator ColorRoutine(Color target, float duration)
    {
        Color from = targetImage.color;
        float d = Mathf.Max(0.0001f, duration);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / d;
            targetImage.color = Color.Lerp(from, target, Mathf.Clamp01(t));
            yield return null;
        }

        targetImage.color = target;
        colorRoutine = null;
    }

    private static float BackOut(float t, float s)
    {
        t -= 1f;
        return t * t * ((s + 1f) * t + s) + 1f;
    }

    private static float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    // ================= audio / haptics =================

    private void PlayClick()
    {
        if (clickClip != null)
        {
            AudioSource src = audioSource != null ? audioSource : GetSharedSource();
            if (src != null)
                src.PlayOneShot(clickClip, clickVolume);
        }

        if (enableHaptics)
            Haptics.Light();
    }

    private static AudioSource GetSharedSource()
    {
        if (sharedSource != null)
            return sharedSource;

        var go = new GameObject("[UI Click Audio]");
        Object.DontDestroyOnLoad(go);
        sharedSource = go.AddComponent<AudioSource>();
        sharedSource.playOnAwake = false;
        sharedSource.spatialBlend = 0f;
        return sharedSource;
    }
}

/// <summary>
/// Light haptic wrapper. Platform-gated; no-op where unsupported.
/// </summary>
internal static class Haptics
{
    public static void Light()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
        // iOS: no built-in light-impact API; use a plugin
        // (UIImpactFeedbackGenerator .light) or leave empty.
#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
        // Desktop: mouse clicks have no vibration.
        // For gamepad rumble call XInputSetState.
#endif
    }
}
