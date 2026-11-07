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

    public bool IsGrounded { get { return onGround; } }
    public float AudioMoveInput { get { return controlEnabled ? xInput : 0f; } }
    public Collider2D GroundCollider { get; private set; }

    SpriteRenderer sprite;
    Collider2D bodyCollider;
    CapsuleCollider2D capsule;
    Vector2 capsuleSize;
    float xInput;
    bool jumpPressed;
    float jumpLock;
<<<<<<< Updated upstream
=======
    bool onGround;
    bool airFlipUsed;
    bool controlEnabled = true;
    public int jumpHoldFrame = 3;
    public float jumpFinishDistance = 1.5f;
    public float blockBounceHeight = 1f / 3f;
    public bool autoFaceChange = true;
    public float faceChangeCooldown = 0.25f;
    public float faceSnapSlide = 1.5f;
    public float wallGrabSpeed = 10f;
    AnimationClip jumpClip;
    float jumpHoldTime;
    Rigidbody2D rideBody;
    Vector2 rideLastPos;
    bool wasRiding;
    float nextFaceChange;
    bool wasGrounded;
    bool wasOnBlock;
    Collider2D lastGroundCollider;
    bool hasPendingWall;
    Vector2 pendingWallGravity;
    Vector2 pendingWallPoint;
    Vector2 pendingVelocity;
>>>>>>> Stashed changes

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
        if (GetComponent<CastlePlayerAudio>() == null)
        {
            gameObject.AddComponent<CastlePlayerAudio>();
        }
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

        if (!cursorMode && !blockSelected)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || (pad && PadInput.UpPressed))
            {
                ChangeGravity(Vector2.up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || (pad && PadInput.DownPressed))
            {
                ChangeGravity(Vector2.down);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || (pad && PadInput.LeftPressed))
            {
                ChangeGravity(Vector2.left);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || (pad && PadInput.RightPressed))
            {
                ChangeGravity(Vector2.right);
            }
        }

        xInput = 0f;

        if (!cursorMode)
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
    }

    void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        if (hasPendingWall)
        {
            ApplyPendingWallGrab();
            return;
        }

        jumpLock = jumpLock - Time.fixedDeltaTime;

        if (jumpLock < 0f)
        {
            jumpLock = 0f;
        }

        Vector2 right = new Vector2(-gravityDirection.y, gravityDirection.x);
<<<<<<< Updated upstream
=======
        Rigidbody2D support;
        onGround = CheckGround(out support);
        Rigidbody2D platform = null;

        if (onGround && support != null && support != rb && support.bodyType != RigidbodyType2D.Static)
        {
            platform = support;
        }

        bool onBlock = onGround && support != null && support.GetComponent<GravityBlock>() != null;

        if (autoFaceChange && !onGround && wasGrounded && wasOnBlock && jumpLock <= 0f && Mathf.Abs(xInput) > 0.1f && Time.time >= nextFaceChange)
        {
            ApplyFaceChange();
            return;
        }

        if (onBlock && support != null)
        {
            lastGroundCollider = support.GetComponentInChildren<Collider2D>();
        }

        wasGrounded = onGround;
        wasOnBlock = onBlock;

        CarryPlatform(platform);
        bool jumping = jumpPressed && onGround;

        if (!jumping)
        {
            float keep = Mathf.Sqrt(Mathf.Clamp01(blockBounceHeight));

            if (platform != null)
            {
                float up = Vector2.Dot(rb.velocity, -gravityDirection);
                float platformUp = Vector2.Dot(platform.velocity, -gravityDirection);

                if (up > platformUp)
                {
                    rb.velocity = rb.velocity + gravityDirection * ((up - platformUp) * (1f - keep));
                }
            }
            else if (wasRiding)
            {
                float up = Vector2.Dot(rb.velocity, -gravityDirection);

                if (up > 0f)
                {
                    rb.velocity = rb.velocity + gravityDirection * (up * (1f - keep));
                }
            }
        }

>>>>>>> Stashed changes
        float gravitySpeed = Vector2.Dot(rb.velocity, gravityDirection);
        rb.velocity = right * xInput * moveSpeed + gravityDirection * gravitySpeed;

        bool grounded = CheckGround();

        if (jumpPressed && grounded)
        {
            rb.velocity = right * xInput * moveSpeed - gravityDirection * jumpForce;
            jumpLock = 0.15f;
            grounded = false;

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
            anim.SetBool("isGrounded", grounded);
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

<<<<<<< Updated upstream
    bool CheckGround()
    {
=======
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
        if (airFlipUsed)
        {
            return;
        }

        airFlipUsed = true;
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!autoFaceChange || !controlEnabled || rb == null)
        {
            return;
        }

        if (hasPendingWall || Time.time < nextFaceChange)
        {
            return;
        }

        if (collision.collider == null || collision.collider.isTrigger)
        {
            return;
        }

        if (collision.relativeVelocity.magnitude < wallGrabSpeed)
        {
            return;
        }

        int i = 0;

        while (i < collision.contactCount)
        {
            ContactPoint2D contact = collision.GetContact(i);
            Vector2 toWall = contact.point - (Vector2)rb.position;

            if (toWall.sqrMagnitude > 0.0001f)
            {
                Vector2 axis = SnapAxis(toWall);

                if (Mathf.Abs(Vector2.Dot(axis, gravityDirection)) <= 0.5f)
                {
                    hasPendingWall = true;
                    pendingWallGravity = axis;
                    pendingWallPoint = contact.point;
                    pendingVelocity = rb.velocity;
                    return;
                }
            }

            i = i + 1;
        }
    }

    Vector2 SnapAxis(Vector2 v)
    {
        if (Mathf.Abs(v.x) >= Mathf.Abs(v.y))
        {
            return v.x >= 0f ? Vector2.right : Vector2.left;
        }

        return v.y >= 0f ? Vector2.up : Vector2.down;
    }

    void GetBodyExtents(Vector2 dir, out float ex, out float ey)
    {
        if (capsule != null)
        {
            if (Mathf.Abs(dir.x) > 0.5f)
            {
                ex = capsuleSize.y * 0.5f;
                ey = capsuleSize.x * 0.5f;
            }
            else
            {
                ex = capsuleSize.x * 0.5f;
                ey = capsuleSize.y * 0.5f;
            }
        }
        else if (bodyCollider != null)
        {
            ex = bodyCollider.bounds.extents.x;
            ey = bodyCollider.bounds.extents.y;
        }
        else
        {
            ex = 0.3f;
            ey = 0.3f;
        }
    }

    void ApplyPendingWallGrab()
    {
        hasPendingWall = false;
        Vector2 nextGravity = pendingWallGravity;
        Vector2 point = pendingWallPoint;
        Vector2 velocityBefore = pendingVelocity;
        nextFaceChange = Time.time + faceChangeCooldown;
        wasGrounded = false;
        wasOnBlock = false;
        ChangeGravity(nextGravity);

        float ex;
        float ey;
        GetBodyExtents(nextGravity, out ex, out ey);
        Vector2 outward = -nextGravity;
        float halfNormal = Mathf.Abs(outward.x) * ex + Mathf.Abs(outward.y) * ey;
        rb.position = point + outward * (halfNormal + 0.02f);

        Vector2 tangent = new Vector2(-nextGravity.y, nextGravity.x);
        float slide = Vector2.Dot(velocityBefore, tangent);
        slide = Mathf.Clamp(slide, -moveSpeed * 2f, moveSpeed * 2f);
        rb.velocity = tangent * slide;
    }

    void ApplyFaceChange()
    {
        Vector2 oldGravity = gravityDirection;
        Vector2 move = new Vector2(-oldGravity.y, oldGravity.x) * Mathf.Sign(xInput);
        Vector2 normal = -oldGravity;
        Vector2 nextGravity = -move;

        nextFaceChange = Time.time + faceChangeCooldown;
        wasGrounded = false;
        wasOnBlock = false;

        Collider2D supportCollider = lastGroundCollider;
        lastGroundCollider = null;
        ChangeGravity(nextGravity);

        float ex;
        float ey;

        if (capsule != null)
        {
            if (Mathf.Abs(nextGravity.x) > 0.5f)
            {
                ex = capsuleSize.y * 0.5f;
                ey = capsuleSize.x * 0.5f;
            }
            else
            {
                ex = capsuleSize.x * 0.5f;
                ey = capsuleSize.y * 0.5f;
            }
        }
        else if (bodyCollider != null)
        {
            ex = bodyCollider.bounds.extents.x;
            ey = bodyCollider.bounds.extents.y;
        }
        else
        {
            ex = 0.3f;
            ey = 0.3f;
        }

        if (supportCollider != null)
        {
            Bounds supportBounds = supportCollider.bounds;
            float halfMove = Mathf.Abs(move.x) * ex + Mathf.Abs(move.y) * ey;
            float halfNormal = Mathf.Abs(normal.x) * ex + Mathf.Abs(normal.y) * ey;
            float supportMove = Mathf.Abs(move.x) * supportBounds.extents.x + Mathf.Abs(move.y) * supportBounds.extents.y;
            float supportNormal = Mathf.Abs(normal.x) * supportBounds.extents.x + Mathf.Abs(normal.y) * supportBounds.extents.y;
            Vector2 supportCenter = supportBounds.center;
            float centerMove = Vector2.Dot(supportCenter, move) + supportMove + halfMove + 0.02f;
            float centerNormal = Vector2.Dot(supportCenter, normal) + supportNormal - halfNormal - 0.02f;
            rb.position = move * centerMove + normal * centerNormal;
        }
        else
        {
            rb.position = rb.position + nextGravity * 0.1f;
        }

        Vector2 newRight = new Vector2(-nextGravity.y, nextGravity.x);
        rb.velocity = newRight * faceSnapSlide;
    }

    void CarryPlatform(Rigidbody2D support)
    {
        if (!onGround || support == null || support == rb || support.bodyType == RigidbodyType2D.Static)
        {
            rideBody = null;
            return;
        }

        if (rideBody == support)
        {
            Vector2 delta = support.position - rideLastPos;
            Vector2 right = new Vector2(-gravityDirection.y, gravityDirection.x);
            Vector2 tangent = right * Vector2.Dot(delta, right);

            if (tangent.sqrMagnitude > 0f)
            {
                rb.position = rb.position + tangent;
            }
        }

        rideBody = support;
        rideLastPos = support.position;
    }

    bool CheckGround()
    {
        Rigidbody2D support;
        return CheckGround(out support);
    }

    bool CheckGround(out Rigidbody2D support)
    {
        support = null;
        GroundCollider = null;

>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
                    return true;
=======
                    if (!found || hits[n].distance < best)
                    {
                        best = hits[n].distance;
                        support = hit.attachedRigidbody;
                        GroundCollider = hit;
                        found = true;
                    }
>>>>>>> Stashed changes
                }

                n = n + 1;
            }

            i = i + 1;
        }

        return false;
    }
}
