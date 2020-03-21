using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OculusShareDownloader
{
    public class Data
    {
        public bool IsChecked { get; private set; } = false;
        public string FileType { get; private set; } = "";
        public string FilePath { get; private set; } = "";
        public string FileName { get; private set; } = "";

        public Data(bool isChecked, string path)
        {
            IsChecked = isChecked;
            FilePath = path;
            FileName = Path.GetFileName(path);
            FileType = GetFileTypeString(path);
        }
        
        private static string GetFileTypeString(string path)
        {
            string extension = Path.GetExtension(path);
            switch (extension)
            {
                case ".mp4":
                    return "VideoShot";
                case ".jpg":
                    return "ScreenShot";
                default:
                    return "?";
            }
        }
    }
}
