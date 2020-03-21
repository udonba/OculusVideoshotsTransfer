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
        public string FileType { get; private set; } = "";
        public string FilePath { get; private set; } = "";
        public string FileName { get; private set; } = "";
        public DateTime Date { get; private set; } = new DateTime();

        public Data(string path)
        {
            FilePath = path;
            FileName = Path.GetFileName(path);
            FileType = GetFileTypeString(path);
            Date = ConvertToDate(path);
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

        /// <summary>
        /// ファイル名を日時に変換
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static DateTime ConvertToDate(string fileName)
        {
            string[] s = Path.GetFileNameWithoutExtension(fileName).Split('-');
            if (s.Length == 3 && DateTime.TryParseExact(s[1] + s[2], "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                Console.WriteLine(string.Format("[{0}] -> [{1}]", fileName, date));
                return date;
            }
            return new DateTime();
        }

        /// <summary>
        /// ファイル名に含まれる日時で降順ソート
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int CompareByDateStringDesc(Data a, Data b)
        {
            //文字列の長さを比較する
            return DateTime.Compare(b.Date, a.Date);
        }
    }
}
