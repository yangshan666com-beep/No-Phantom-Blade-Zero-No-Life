using UnityEngine;

public class GravityBlock : MonoBehaviour
{
    public float gravityForce = 30f;
    public Color selectedColor = new Color(0.6f, 0.85f, 1f);

    Rigidbody2D body;
    SpriteRenderer sprite;
    Color normalColor;
    Vector2 gravityDirection = Vector2.down;
    CastleBlockAudio blockAudio;

    void OnEnable()
    {
        blockAudio = GetComponent<CastleBlockAudio>();
        if (blockAudio == null) blockAudio = gameObject.AddComponent<CastleBlockAudio>();
    }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        body.gravityScale = 0f;
        normalColor = sprite.color;
        blockAudio = GetComponent<CastleBlockAudio>();
        if (blockAudio == null) blockAudio = gameObject.AddComponent<CastleBlockAudio>();
        ApplyNameDirection();
    }

    void Start()
    {
        ApplyNameDirection();
    }

    void ApplyNameDirection()
    {
        string n = gameObject.name.ToUpperInvariant();

        if (n.Contains("GIW"))
        {
            gravityDirection = Vector2.up;
        }
        else if (n.Contains("GIA"))
        {
            gravityDirection = Vector2.left;
        }
        else if (n.Contains("GIS"))
        {
            gravityDirection = Vector2.down;
        }
        else if (n.Contains("GID"))
        {
            gravityDirection = Vector2.right;
        }
    }

    void FixedUpdate()
    {
        body.AddForce(gravityDirection * gravityForce, ForceMode2D.Force);
    }

    public void SetDirection(Vector2 dir)
    {
        Vector2 next = dir.normalized;
        bool changed = next.sqrMagnitude > 0.5f && Vector2.Dot(gravityDirection, next) < 0.99f;
        gravityDirection = next;
        if (changed && Time.timeSinceLevelLoad > 0.2f)
        {
            if (blockAudio == null) blockAudio = GetComponent<CastleBlockAudio>();
            if (blockAudio == null) blockAudio = gameObject.AddComponent<CastleBlockAudio>();
            blockAudio.BeginMove(next);
        }
    }

    public void Select(bool on)
    {
        if (on)
        {
            sprite.color = selectedColor;
        }
        else
        {
            sprite.color = normalColor;
        }
    }
}
