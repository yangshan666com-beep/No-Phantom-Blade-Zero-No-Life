using UnityEngine;

public class GravityBlock : MonoBehaviour
{
    public float gravityForce = 30f;
    public float holdMass = 200f;
    public Color selectedColor = new Color(0.6f, 0.85f, 1f);

    [Header("初始重力方向( 0,0 = 按物体名字判断，兼容旧方块)")]
    public Vector2 initialDirection = Vector2.down;
    Rigidbody2D body;
    SpriteRenderer sprite;
    Color normalColor;
    Vector2 gravityDirection = Vector2.down;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        body.gravityScale = 0f;
        body.mass = holdMass;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        normalColor = sprite.color;
        // 优先用 Inspector 里设的方向；没设就退回按名字判断
        if (initialDirection.sqrMagnitude > 0.01f)
        {
            gravityDirection = initialDirection.normalized;
        }
        else
        {
            ApplyNameDirection();
        }
        initialDirection = gravityDirection;
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
        body.AddForce(gravityDirection * gravityForce * body.mass, ForceMode2D.Force);
    }

    public void SetDirection(Vector2 dir)
    {
        gravityDirection = dir.normalized;
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
