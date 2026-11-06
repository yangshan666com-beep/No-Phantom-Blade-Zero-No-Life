using UnityEngine;

public class GravityBlock : MonoBehaviour
{
    public float gravityForce = 30f;
    public float holdMass = 200f;
    public Color selectedColor = new Color(0.6f, 0.85f, 1f);

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
