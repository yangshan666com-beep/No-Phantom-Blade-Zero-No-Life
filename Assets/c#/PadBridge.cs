using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class PadBridge
{
    static Process process;
    static Thread thread;
    static bool started;

    static volatile bool ready;
    static volatile bool hasState;
    static volatile float stickX;
    static volatile float stickY;
    static volatile float triggerL;
    static volatile float rightX;
    static volatile float rightY;

    static int crossTime;
    static int triangleTime;
    static int upTime;
    static int downTime;
    static int leftTime;
    static int rightTime;

    static int crossSeen;
    static int triangleSeen;
    static int upSeen;
    static int downSeen;
    static int leftSeen;
    static int rightSeen;

    public static bool Ready
    {
        get
        {
            Start();
            return ready && hasState;
        }
    }

    public static Vector2 LeftStick
    {
        get
        {
            float x = stickX;
            float y = stickY;

            if (x * x + y * y < 0.04f)
            {
                return Vector2.zero;
            }

            return new Vector2(x, y);
        }
    }

    public static Vector2 RightStick
    {
        get
        {
            float x = rightX;
            float y = rightY;

            if (x * x + y * y < 0.04f)
            {
                return Vector2.zero;
            }

            return new Vector2(x, y);
        }
    }

    public static float LeftTrigger
    {
        get { return triggerL; }
    }

    public static bool CrossPressed
    {
        get { return UsePress(ref crossTime, ref crossSeen); }
    }

    public static bool TrianglePressed
    {
        get { return UsePress(ref triangleTime, ref triangleSeen); }
    }

    public static bool UpPressed
    {
        get { return UsePress(ref upTime, ref upSeen); }
    }

    public static bool DownPressed
    {
        get { return UsePress(ref downTime, ref downSeen); }
    }

    public static bool LeftPressed
    {
        get { return UsePress(ref leftTime, ref leftSeen); }
    }

    public static bool RightPressed
    {
        get { return UsePress(ref rightTime, ref rightSeen); }
    }

    static bool UsePress(ref int pressTime, ref int seenTime)
    {
        int time = Volatile.Read(ref pressTime);

        if (time <= seenTime)
        {
            return false;
        }

        seenTime = time;

        int now = Environment.TickCount;
        int age = unchecked(now - time);

        if (age < 0 || age > 250)
        {
            return false;
        }

        return true;
    }

    static void Start()
    {
        if (started)
        {
            return;
        }

        started = true;
        string path = FindFile();

        if (path == null)
        {
            Debug.LogWarning("找不到 PadBridge/gamepadbridge");
            return;
        }

        KillOld();

        try
        {
            ProcessStartInfo info = new ProcessStartInfo(path);
            info.UseShellExecute = false;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.CreateNoWindow = true;
            info.WorkingDirectory = Path.GetDirectoryName(path);

            process = Process.Start(info);

            if (process == null)
            {
                started = false;
                return;
            }

            thread = new Thread(ReadLoop);
            thread.IsBackground = true;
            thread.Start();
        }
        catch (Exception e)
        {
            Debug.LogWarning("启动 PadBridge 失败: " + e.Message);
            process = null;
            started = false;
        }
    }

    static void KillOld()
    {
        try
        {
            ProcessStartInfo info = new ProcessStartInfo("/usr/bin/pkill");
            info.Arguments = "-f PadBridge/gamepadbridge";
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            Process old = Process.Start(info);

            if (old != null)
            {
                old.WaitForExit(200);
                old.Dispose();
            }
        }
        catch
        {
        }
    }

    static string FindFile()
    {
        try
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            string path = Path.Combine(root, "PadBridge", "gamepadbridge");

            if (File.Exists(path))
            {
                return path;
            }

            string stream = Path.Combine(Application.streamingAssetsPath, "gamepadbridge");

            if (File.Exists(stream))
            {
                return stream;
            }
        }
        catch
        {
        }

        return null;
    }

    static void ReadLoop()
    {
        try
        {
            string line;

            while ((line = process.StandardOutput.ReadLine()) != null)
            {
                if (line.StartsWith("PAD "))
                {
                    ReadState(line);
                }
                else if (line.StartsWith("P "))
                {
                    ReadPress(line.Substring(2).Trim());
                }
                else if (line.StartsWith("READY"))
                {
                    ready = true;
                    Debug.Log("手柄已连接: " + line);
                }
                else if (line.StartsWith("DISCONNECTED"))
                {
                    ready = false;
                    hasState = false;
                    Debug.LogWarning("手柄已断开");
                }
            }
        }
        catch
        {
        }

        ready = false;
        hasState = false;
    }

    static void ReadState(string line)
    {
        string[] parts = line.Split(' ');

        if (parts.Length < 10)
        {
            return;
        }

        float x;
        float y;
        float l2;
        float rx = 0f;
        float ry = 0f;

        if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out x))
        {
            return;
        }
        if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out y))
        {
            return;
        }
        if (!float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out l2))
        {
            return;
        }

        if (parts.Length >= 12)
        {
            float.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out rx);
            float.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out ry);
        }

        stickX = x;
        stickY = y;
        triggerL = l2;
        rightX = rx;
        rightY = ry;
        hasState = true;
    }

    static void ReadPress(string name)
    {
        int now = Environment.TickCount;

        if (name == "cross")
        {
            Volatile.Write(ref crossTime, now);
        }
        else if (name == "triangle")
        {
            Volatile.Write(ref triangleTime, now);
        }
        else if (name == "up")
        {
            Volatile.Write(ref upTime, now);
        }
        else if (name == "down")
        {
            Volatile.Write(ref downTime, now);
        }
        else if (name == "left")
        {
            Volatile.Write(ref leftTime, now);
        }
        else if (name == "right")
        {
            Volatile.Write(ref rightTime, now);
        }
    }

    public static void Stop()
    {
        try
        {
            if (process != null && !process.HasExited)
            {
                process.Kill();
            }
        }
        catch
        {
        }

        try
        {
            if (process != null)
            {
                process.Dispose();
            }
        }
        catch
        {
        }

        process = null;
        ready = false;
        hasState = false;
        started = false;
    }
}

public class PadBridgeRunner : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        GameObject go = new GameObject("PadBridgeRunner");
        go.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(go);
        go.AddComponent<PadBridgeRunner>();
    }

    void OnApplicationQuit()
    {
        PadBridge.Stop();
    }

    void OnDestroy()
    {
        PadBridge.Stop();
    }
}
