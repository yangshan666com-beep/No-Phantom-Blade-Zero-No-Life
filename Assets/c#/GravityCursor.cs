using UnityEngine;

public class GravityCursor : MonoBehaviour
{
    public float radius = 2f;
    public float cursorSpeed = 24f;

    public static GravityCursor Instance { get; private set; }
    public bool CursorMode { get; private set; }
    public bool Selected { get; private set; }

    Player1 player;
    GravityBlock target;
    SpriteRenderer sprite;
    Vector3 lastMouse;
    bool heldBefore;

    void Awake()
    {
        Instance = this;
        player = FindObjectOfType<Player1>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Cursor.visible = false;
        transform.position = CenterPosition();
        SetVisible(false);
    }

    public void SetPlayer(Player1 newPlayer)
    {
        player = newPlayer;
    }

    void OnDestroy()
    {
        Instance = null;
        Cursor.visible = true;
        ClearTarget();
    }

    void Update()
    {
        bool pad = PadInput.Connected;
        bool held = pad && PadInput.CursorMode;

        CursorMode = held;

        if (pad)
        {
            if (held)
            {
                if (!heldBefore)
                {
                    transform.position = CenterPosition();
                }

                Vector2 stick = PadInput.LeftStick;
                Vector3 move = new Vector3(stick.x, stick.y, 0f) * cursorSpeed * Time.deltaTime;
                transform.position = transform.position + move;
                SetVisible(true);
                UpdateSelection(true);
            }
            else
            {
                ClearTarget();
                Selected = false;
                SetVisible(false);
            }

            heldBefore = held;
            return;
        }

        heldBefore = false;
        SetVisible(true);

        bool mouseMoved = Input.mousePosition != lastMouse;

        if (mouseMoved)
        {
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mouse.x, mouse.y, 0f);
        }

        lastMouse = Input.mousePosition;
        UpdateSelection(false);
    }

    void UpdateSelection(bool pad)
    {
        GravityBlock next = FindTarget();

        if (next != target)
        {
            if (target != null)
            {
                target.Select(false);
            }

            target = next;
        }

        Selected = target != null;

        if (!Selected)
        {
            return;
        }

        target.Select(true);

        if (Input.GetKeyDown(KeyCode.UpArrow) || (pad && PadInput.UpPressed))
        {
            target.SetDirection(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || (pad && PadInput.DownPressed))
        {
            target.SetDirection(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || (pad && PadInput.LeftPressed))
        {
            target.SetDirection(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || (pad && PadInput.RightPressed))
        {
            target.SetDirection(Vector2.right);
        }
    }

    void ClearTarget()
    {
        if (target != null)
        {
            target.Select(false);
            target = null;
        }
    }

    void SetVisible(bool on)
    {
        if (sprite == null)
        {
            sprite = GetComponent<SpriteRenderer>();
        }

        if (sprite != null)
        {
            sprite.enabled = on;
        }
    }

    Vector3 CenterPosition()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            return Vector3.zero;
        }

        Vector3 center = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
        return new Vector3(center.x, center.y, 0f);
    }

    GravityBlock FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        int i = 0;

        while (i < hits.Length)
        {
            if (hits[i].gameObject.name.ToUpper().Contains("GI"))
            {
                GravityBlock block = hits[i].GetComponent<GravityBlock>();

                if (block != null)
                {
                    return block;
                }
            }

            i = i + 1;
        }

        return null;
    }
}
