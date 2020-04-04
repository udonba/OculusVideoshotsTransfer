using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static OculusShareDownloader.Extension;

namespace OculusShareDownloader
{
    public static class Downloader
    {
        public const string ScreenshotsDirectoryPath = "/sdcard/oculus/screenshots/";
        public const string VideoshotsDirectoryPath = "/sdcard/oculus/videoshots/";

        public const string DefaultDownloadDirectoryPath = "D:/Downloads/";
        
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

        /// <summary>
        /// ファイルをダウンロードする
        /// </summary>
        /// <param name="files"></param>
        public static async Task DownloadFiles(string[] files)
        {
            for (int i = 0; i < files.Length; i++)
            {
                await DownloadFile(files[i]);
            }
        }

        private static async Task DownloadFile(string srcPath, string dstPath = "")
        {
            if (dstPath == "")
            {
                dstPath = DefaultDownloadDirectoryPath + Path.GetFileName(srcPath);
            }

            var tcs = new TaskCompletionSource<int>();
            ProcessStartInfo info = new ProcessStartInfo()
            {
                FileName = "adb",
                Arguments = string.Format("pull {0} {1}", srcPath, dstPath),
                CreateNoWindow = true,
                UseShellExecute = false,
            };

            var process = new Process();
            process.StartInfo = info;
            await RunProcessAsync(process);
        }

        private static Task<int> RunProcessAsync(Process process)
        {
            var tcs = new TaskCompletionSource<int>();
            process.EnableRaisingEvents = true;

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
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

        /// <summary>
        /// VideoshotsとScreenshots一覧を取得
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public static string[] GetAllFiles(string deviceName = "")
        {
            var videos = Downloader.GetFiles(Downloader.VideoshotsDirectoryPath, deviceName);
            var screens = Downloader.GetFiles(Downloader.ScreenshotsDirectoryPath, deviceName);
            var array = new string[videos.Length + screens.Length];
            Array.Copy(videos, 0, array, 0, videos.Length);
            Array.Copy(screens, 0, array, videos.Length, screens.Length);

            return array;
        }
    }
}
