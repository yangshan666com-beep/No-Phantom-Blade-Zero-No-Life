using UnityEngine;

public class GravityCursor : MonoBehaviour
{
    public float radius = 2f;
    public float cursorSpeed = 24f;
    public float cursorScale = 2f;

    public static GravityCursor Instance { get; private set; }
    public bool CursorMode { get; private set; }
    public bool Selected { get; private set; }
    public GravityBlock Target
    {
        get { return target; }
    }

    public Player1 Player
    {
        get { return selectedPlayer; }
    }

    Player1 player;
    GravityBlock target;
    Player1 selectedPlayer;
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

        if (cursorScale < 0.1f)
        {
            cursorScale = 2f;
        }

        transform.localScale = Vector3.one * cursorScale;
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
        GravityBlock nextBlock = FindBlock();
        Player1 nextPlayer = nextBlock == null ? FindPlayer() : null;

        if (nextBlock != target)
        {
            if (target != null)
            {
                target.Select(false);
            }

            target = nextBlock;
        }

        if (nextPlayer != selectedPlayer)
        {
            if (selectedPlayer != null)
            {
                selectedPlayer.Select(false);
            }

            selectedPlayer = nextPlayer;
        }

        Selected = target != null || selectedPlayer != null;

        if (target != null)
        {
            target.Select(true);
        }

        if (selectedPlayer != null)
        {
            selectedPlayer.Select(true);
        }

        if (!Selected)
        {
            return;
        }

        if (pad && PadInput.CursorMode)
        {
            return;
        }
    }

    void ClearTarget()
    {
        if (target != null)
        {
            target.Select(false);
            target = null;
        }

        if (selectedPlayer != null)
        {
            selectedPlayer.Select(false);
            selectedPlayer = null;
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

    GravityBlock FindBlock()
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

    Player1 FindPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        int i = 0;

        while (i < hits.Length)
        {
            Player1 found = hits[i].GetComponentInParent<Player1>();

            if (found != null)
            {
                return found;
            }

            i = i + 1;
        }

        return null;
    }
}
