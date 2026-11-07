using UnityEngine;

public static class PadInput
{
    public static bool Connected
    {
        get { return PadBridge.Ready; }
    }

    public static Vector2 LeftStick
    {
        get { return PadBridge.LeftStick; }
    }

    public static Vector2 RightStick
    {
        get { return PadBridge.RightStick; }
    }

    public static float LeftTrigger
    {
        get { return PadBridge.LeftTrigger; }
    }

    public static bool CursorMode
    {
        get
        {
            if (LeftTrigger > 0.5f)
            {
                return true;
            }

            return false;
        }
    }

    public static bool CrossPressed
    {
        get { return PadBridge.CrossPressed; }
    }

    public static bool TrianglePressed
    {
        get { return PadBridge.TrianglePressed; }
    }

    public static bool UpPressed
    {
        get { return PadBridge.UpPressed; }
    }

    public static bool DownPressed
    {
        get { return PadBridge.DownPressed; }
    }

    public static bool LeftPressed
    {
        get { return PadBridge.LeftPressed; }
    }

    public static bool RightPressed
    {
        get { return PadBridge.RightPressed; }
    }
}
