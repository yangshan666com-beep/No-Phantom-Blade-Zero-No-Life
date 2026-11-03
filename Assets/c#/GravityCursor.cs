using UnityEngine;

public class GravityCursor : MonoBehaviour
{
    public float radius = 2f;
    public float cursorSpeed = 6f;
    public GravityBlock target;

    public static GravityCursor Instance { get; private set; }
    public bool CursorMode { get; private set; }
    public bool ConfirmedSelection { get; private set; }

    Player1 player;
    Vector3 lastMouse;

    void Awake()
    {
        Instance = this;
        player = FindObjectOfType<Player1>();

        if (target == null)
        {
            target = FindObjectOfType<GravityBlock>();
        }
    }

    void Start()
    {
        Cursor.visible = false;

        if (target != null)
        {
            transform.position = new Vector3(target.transform.position.x, target.transform.position.y, 0f);
        }
    }

    void OnDestroy()
    {
        Instance = null;
        Cursor.visible = true;

        if (target != null)
        {
            target.Select(false);
        }
    }

    void Update()
    {
        bool pad = PadInput.Connected;
        CursorMode = pad && PadInput.CursorMode;

        if (CursorMode)
        {
            Vector2 stick = PadInput.LeftStick;
            Vector3 move = new Vector3(stick.x, stick.y, 0f) * cursorSpeed * Time.deltaTime;
            transform.position = transform.position + move;
        }
        else
        {
            bool mouseMoved = Input.mousePosition != lastMouse;

            if (!pad || mouseMoved)
            {
                Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position = new Vector3(mouse.x, mouse.y, 0f);
            }
        }

        lastMouse = Input.mousePosition;

        if (target == null)
        {
            return;
        }

        bool over = CheckTarget();

        if (CursorMode && PadInput.CrossPressed && over && !ConfirmedSelection)
        {
            ConfirmedSelection = true;
        }

        if (ConfirmedSelection)
        {
            target.Select(true);

            if (!PlayerSelectingGravity())
            {
                if (PadInput.UpPressed)
                {
                    Choose(Vector2.up);
                }
                else if (PadInput.DownPressed)
                {
                    Choose(Vector2.down);
                }
                else if (PadInput.LeftPressed)
                {
                    Choose(Vector2.left);
                }
                else if (PadInput.RightPressed)
                {
                    Choose(Vector2.right);
                }
            }

            return;
        }

        target.Select(over);

        if (!over)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            return;
        }
        if (PlayerSelectingGravity())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Choose(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Choose(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Choose(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Choose(Vector2.right);
        }
    }

    void Choose(Vector2 dir)
    {
        target.SetDirection(dir);
        target.Select(false);
        ConfirmedSelection = false;
    }

    bool PlayerSelectingGravity()
    {
        if (player == null)
        {
            return false;
        }
        if (player.choosingGravity)
        {
            return true;
        }
        if (player.gravitySelectionFrame == Time.frameCount)
        {
            return true;
        }

        return false;
    }

    bool CheckTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        int i = 0;

        while (i < hits.Length)
        {
            if (hits[i].gameObject == target.gameObject)
            {
                return true;
            }

            i = i + 1;
        }

        return false;
    }
}
