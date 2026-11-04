using UnityEngine;

public class GravityCursor : MonoBehaviour
{
    public float radius = 2f;
    public float cursorSpeed = 6f;

    public static GravityCursor Instance { get; private set; }
    public bool CursorMode { get; private set; }
    public bool Selected { get; private set; }

    Player1 player;
    GravityBlock target;
    Vector3 lastMouse;

    void Awake()
    {
        Instance = this;
        player = FindObjectOfType<Player1>();
    }

    void Start()
    {
        Cursor.visible = false;

        if (player != null)
        {
            transform.position = player.transform.position + new Vector3(0f, 1.5f, 0f);
        }
        else
        {
            transform.position = new Vector3(0f, 0f, 0f);
        }
    }

    void OnDestroy()
    {
        Instance = null;
        Cursor.visible = true;

        if (target != null)
        {
            target.Select(false);
            target = null;
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
