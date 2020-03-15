using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OculusShareDownloader.Extension;

namespace OculusShareDownloader
{
    public static class Downloader
    {
        public const string PATH_SCREENSHOTS = "/sdcard/oculus/screenshots/";
        public const string PATH_VIDEOSHOTS = "/sdcard/oculus/videoshots/";

        public static string GetCmdOutput(string exe, string arg)
        {
#if DEBUG
            Console.WriteLine(string.Format(@"""{0} {1}""", exe, arg));
#endif
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = false;
            p.StartInfo.CreateNoWindow = true;

            p.StartInfo.FileName = exe;
            p.StartInfo.Arguments = arg;
            p.Start();

            string results = p.StandardOutput.ReadToEnd();

            p.WaitForExit();
            p.Close();

            return results;
        }

        public static string[] GetDevices()
        {
            var names = new List<string>();
            var lines = GetCmdOutput("adb", "devices").SplitToLine();
            for (int i = 0; i < lines.Length; i++)
            {
                if (TryParseDeviceName(lines[i], out string name))
                {
                    names.Add(name);
                }
            }
            return names.ToArray();
        }

        public static string[] GetFiles(string directory, string deviceName = "")
        {
            string arg = "shell ls " + directory;
            if (deviceName != "")
                arg = string.Format("-s {0} {1}", deviceName, arg);

            var lines = GetCmdOutput("adb", arg).SplitToLine();
            var paths = new string[lines.Length];
            if (!directory.EndsWith("/"))
                directory += "/";

            for (int i = 0; i < lines.Length; i++)
            {
                paths[i] = directory + lines[i];
            }
            return paths;
        }
    }
}
