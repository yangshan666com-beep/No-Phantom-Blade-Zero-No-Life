using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PadBridgeBuild : IPostprocessBuildWithReport
{
    public int callbackOrder
    {
        get { return 0; }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.StandaloneOSX)
        {
            return;
        }

        string appPath = report.summary.outputPath;

        if (!appPath.EndsWith(".app"))
        {
            appPath = appPath + ".app";
        }

        string source = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "PadBridge", "gamepadbridge");

        if (!File.Exists(source))
        {
            Debug.LogWarning("PadBridgeBuild: 找不到 PadBridge/gamepadbridge，打包后手柄将不可用");
            return;
        }

        string folder = Path.Combine(appPath, "Contents", "Resources", "Data", "StreamingAssets");
        Directory.CreateDirectory(folder);

        string target = Path.Combine(folder, "gamepadbridge");
        File.Copy(source, target, true);

        Run("/bin/chmod", "+x \"" + target + "\"");
        Run("/usr/bin/xattr", "-cr \"" + appPath + "\"");

        Debug.Log("PadBridgeBuild: 已把 gamepadbridge 复制进包内 -> " + target);
    }

    static void Run(string file, string args)
    {
        ProcessStartInfo info = new ProcessStartInfo(file, args);
        info.UseShellExecute = false;
        info.CreateNoWindow = true;

        Process p = Process.Start(info);

        if (p != null)
        {
            p.WaitForExit();
            p.Dispose();
        }
    }
}
