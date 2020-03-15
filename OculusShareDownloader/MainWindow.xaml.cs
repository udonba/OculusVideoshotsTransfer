using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static OculusShareDownloader.Extension;

namespace OculusShareDownloader
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var name = GetDeviceNameTest();
            if (name != "")
            {
                GetScreenshotsTest(name);
                GetVideoshotsTest(name);
            }
        }

        private string GetDeviceNameTest()
        {
            Console.WriteLine("Get device names...");
            var lines = Downloader.GetDevices();
            for (int i = 0; i < lines.Length; i++)
            {
                Console.WriteLine(lines[i]);
            }
            if (lines.Length != 0)
                return lines[0];
            else
                return "";
        }

        private void GetScreenshotsTest(string deviceName = "")
        {
            Console.WriteLine("Get screenshots...");
            var lines = Downloader.GetFiles(Downloader.PATH_SCREENSHOTS, deviceName);
            for (int i = 0; i < lines.Length; i++)
            {
                Console.WriteLine(lines[i]);
            }
        }
        private void GetVideoshotsTest(string deviceName = "")
        {
            Console.WriteLine("Get videoshots...");
            var lines = Downloader.GetFiles(Downloader.PATH_VIDEOSHOTS, deviceName);
            for (int i = 0; i < lines.Length; i++)
            {
                Console.WriteLine(lines[i]);
            }
        }

    }
}
