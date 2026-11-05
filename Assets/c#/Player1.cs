using UnityEngine;

public class Player1 : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 10f;
    public float groundCheckDistance = 0.08f;
    public float gravityForce = 30f;
    public Rigidbody2D rb;
    public Animator anim;
    public Vector2 gravityDirection = Vector2.down;

    SpriteRenderer sprite;
    Collider2D bodyCollider;
    CapsuleCollider2D capsule;
    Vector2 capsuleSize;
    float xInput;
    bool jumpPressed;
    float jumpLock;
    bool onGround;
    bool airFlipUsed;
    bool controlEnabled = true;
    public int jumpHoldFrame = 3;
    public float jumpFinishDistance = 1.5f;
    AnimationClip jumpClip;
    float jumpHoldTime;

    void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }

        sprite = GetComponentInChildren<SpriteRenderer>();
        bodyCollider = GetComponent<Collider2D>();

        if (bodyCollider == null)
        {
            bodyCollider = GetComponentInChildren<Collider2D>();
        }

        capsule = bodyCollider as CapsuleCollider2D;

        if (capsule != null)
        {
            capsuleSize = capsule.size;
        }

        if (rb == null)
        {
            return;
        }

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        ChangeGravity(gravityDirection);
    }

    void Update()
    {
        if (rb == null)
        {
            return;
        }

        bool pad = PadInput.Connected;
        bool cursorMode = pad && PadInput.CursorMode;
        bool blockSelected = GravityCursor.Instance != null && GravityCursor.Instance.Selected;

        if (!cursorMode && !blockSelected && controlEnabled)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || (pad && PadInput.UpPressed))
            {
                TryChangeGravity(Vector2.up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || (pad && PadInput.DownPressed))
            {
                TryChangeGravity(Vector2.down);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || (pad && PadInput.LeftPressed))
            {
                TryChangeGravity(Vector2.left);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || (pad && PadInput.RightPressed))
            {
                TryChangeGravity(Vector2.right);
            }
        }

        xInput = 0f;

        if (!cursorMode && controlEnabled)
        {
            float keyX = 0f;
            float keyY = 0f;
            float stickX = 0f;
            float stickY = 0f;

            if (Input.GetKey(KeyCode.A))
            {
                keyX = keyX - 1f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                keyX = keyX + 1f;
            }
            if (Input.GetKey(KeyCode.W))
            {
                keyY = keyY + 1f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                keyY = keyY - 1f;
            }

            if (pad)
            {
                Vector2 stick = PadInput.LeftStick;
                stickX = stick.x;
                stickY = stick.y;
            }

            Vector2 input = new Vector2(keyX + stickX, keyY + stickY);

            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            Vector2 right = new Vector2(-gravityDirection.y, gravityDirection.x);
            float move = Vector2.Dot(input, right);

            if (Mathf.Abs(gravityDirection.x) > 0.5f)
            {
                float side = keyX + stickX;

                if (side < -1f)
                {
                    side = -1f;
                }
                if (side > 1f)
                {
                    side = 1f;
                }

                move = move + side;
            }

            xInput = Mathf.Clamp(move, -1f, 1f);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                jumpPressed = true;
            }
            if (pad && PadInput.CrossPressed)
            {
                jumpPressed = true;
            }
        }

        if (sprite != null)
        {
            if (xInput > 0f)
            {
                sprite.flipX = false;
            }
            else if (xInput < 0f)
            {
                sprite.flipX = true;
            }
        }

        UpdateJumpAnimation();
    }

    void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        jumpLock = jumpLock - Time.fixedDeltaTime;

        if (jumpLock < 0f)
        {
            jumpLock = 0f;
        }

        Vector2 right = new Vector2(-gravityDirection.y, gravityDirection.x);
        float gravitySpeed = Vector2.Dot(rb.velocity, gravityDirection);
        rb.velocity = right * xInput * moveSpeed + gravityDirection * gravitySpeed;

        onGround = CheckGround();

        if (onGround)
        {
            airFlipUsed = false;
        }

        if (jumpPressed && onGround)
        {
            rb.velocity = right * xInput * moveSpeed - gravityDirection * jumpForce;
            jumpLock = 0.15f;
            onGround = false;

            if (anim != null)
            {
                anim.SetTrigger("Jump");
            }
        }

        jumpPressed = false;
        rb.AddForce(gravityDirection * gravityForce * rb.mass);

        if (anim != null)
        {
            anim.SetBool("ismoving", xInput != 0f);
            anim.SetBool("isGrounded", onGround);
            anim.SetFloat("verticalSpeed", Vector2.Dot(rb.velocity, -gravityDirection));
        }
    }

    void ChangeGravity(Vector2 dir)
    {
        if (rb == null)
        {
            return;
        }

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0f)
            {
                gravityDirection = Vector2.right;
            }
            else
            {
                gravityDirection = Vector2.left;
            }
        }
        else
        {
            if (dir.y > 0f)
            {
                gravityDirection = Vector2.up;
            }
            else
            {
                gravityDirection = Vector2.down;
            }
        }

        float angle = Mathf.Atan2(gravityDirection.x, -gravityDirection.y) * Mathf.Rad2Deg;

        if (anim != null)
        {
            anim.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (capsule != null)
        {
            if (Mathf.Abs(gravityDirection.x) > 0.5f)
            {
                capsule.direction = CapsuleDirection2D.Horizontal;
                capsule.size = new Vector2(capsuleSize.y, capsuleSize.x);
            }
            else
            {
                capsule.direction = CapsuleDirection2D.Vertical;
                capsule.size = capsuleSize;
            }
        }

        rb.rotation = 0f;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        jumpPressed = false;
        jumpLock = 0.1f;
    }

    public void SetControlEnabled(bool on)
    {
        controlEnabled = on;

        if (!on)
        {
            xInput = 0f;
            jumpPressed = false;
        }
    }

    void TryChangeGravity(Vector2 dir)
    {
        if (!CheckGround())
        {
            if (airFlipUsed)
            {
                return;
            }

            airFlipUsed = true;
        }

        ChangeGravity(dir);
    }

    void UpdateJumpAnimation()
    {
        if (anim == null || rb == null)
        {
            return;
        }

        CacheJumpClip();

        if (jumpClip == null)
        {
            return;
        }

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        if (!info.IsName("Playerjump"))
        {
            anim.speed = 1f;
            return;
        }

        if (onGround)
        {
            anim.speed = 1f;
            return;
        }

        float fallSpeed = Vector2.Dot(rb.velocity, gravityDirection);

        if (fallSpeed <= 0.1f)
        {
            anim.speed = 1f;
            return;
        }

        float gap = GroundGap();

        if (gap > jumpFinishDistance)
        {
            anim.speed = 0f;
            anim.Play("Playerjump", 0, jumpHoldTime);
        }
        else
        {
            float progress = 1f - Mathf.Clamp01(gap / jumpFinishDistance);
            float time = Mathf.Lerp(jumpHoldTime, 1f, progress);
            anim.speed = 0f;
            anim.Play("Playerjump", 0, time);
        }
    }

    void CacheJumpClip()
    {
        if (jumpClip != null)
        {
            return;
        }

        RuntimeAnimatorController controller = anim.runtimeAnimatorController;

        if (controller == null)
        {
            return;
        }

        AnimationClip[] clips = controller.animationClips;
        int i = 0;

        while (i < clips.Length)
        {
            if (clips[i] != null && clips[i].name.ToLower().Contains("jump"))
            {
                jumpClip = clips[i];
                break;
            }

            i = i + 1;
        }

        if (jumpClip == null)
        {
            return;
        }

        int frames = Mathf.RoundToInt(jumpClip.length * jumpClip.frameRate) + 1;

        if (frames < 2)
        {
            frames = 2;
        }

        jumpHoldTime = Mathf.Clamp01((jumpHoldFrame - 1f) / (frames - 1f));
    }

    float GroundGap()
    {
        if (bodyCollider == null)
        {
            return 99f;
        }

        Bounds bounds = bodyCollider.bounds;
        Vector2 right = new Vector2(-gravityDirection.y, gravityDirection.x);
        float depth;
        float width;

        if (Mathf.Abs(gravityDirection.x) > 0.5f)
        {
            depth = bounds.extents.x;
        }
        else
        {
            depth = bounds.extents.y;
        }

        if (Mathf.Abs(right.x) > 0.5f)
        {
            width = bounds.extents.x;
        }
        else
        {
            width = bounds.extents.y;
        }

        float best = 99f;
        int i = -1;

        while (i <= 1)
        {
            Vector2 origin = (Vector2)bounds.center + right * width * 0.75f * i;
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, gravityDirection, depth + 3f);
            int n = 0;

            while (n < hits.Length)
            {
                Collider2D hit = hits[n].collider;

                if (hit != null && !hit.isTrigger && hit.attachedRigidbody != rb)
                {
                    float gap = hits[n].distance - depth;

                    if (gap < best)
                    {
                        best = gap;
                    }
                }

                n = n + 1;
            }

            i = i + 1;
        }

        return Mathf.Max(0f, best);
    }

    bool CheckGround()
    {
        if (bodyCollider == null)
        {
            return false;
        }
        if (jumpLock > 0f)
        {
            return false;
        }

        Bounds bounds = bodyCollider.bounds;
        Vector2 right = new Vector2(-gravityDirection.y, gravityDirection.x);
        float depth;
        float width;

        if (Mathf.Abs(gravityDirection.x) > 0.5f)
        {
            depth = bounds.extents.x;
        }
        else
        {
            depth = bounds.extents.y;
        }

        if (Mathf.Abs(right.x) > 0.5f)
        {
            width = bounds.extents.x;
        }
        else
        {
            width = bounds.extents.y;
        }

        int i = -1;

        while (i <= 1)
        {
            Vector2 origin = (Vector2)bounds.center + right * width * 0.75f * i;
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, gravityDirection, depth + groundCheckDistance);
            int n = 0;

            while (n < hits.Length)
            {
                Collider2D hit = hits[n].collider;

                if (hit != null && !hit.isTrigger && hit.attachedRigidbody != rb)
                {
                    return true;
                }

                n = n + 1;
            }

            i = i + 1;
        }

        return false;
    }
}
