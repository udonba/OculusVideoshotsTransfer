using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OculusShareDownloader
{
    public static class Extension
    {
        public static string[] SplitToLine(this string source)
        {
            var rawArray = source.Replace("\r", "").Split('\n');
            var lines = new List<string>();
            for (int i = 0; i < rawArray.Length; i++)
            {
                if (rawArray[i] != "")
                    lines.Add(rawArray[i]);
            }
            return lines.ToArray();
        }

        public static bool TryParseDeviceName(string source, out string deviceName)
        {
            deviceName = "";

            if (!source.Contains("device") || !source.Contains("\t"))
                return false;

            var s = source.Split('\t');
            if (s.Length < 1)
                return false;

            deviceName = s[0];
            return true;
        }
    }
}
