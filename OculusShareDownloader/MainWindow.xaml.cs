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

            Button_Click(button_UpdateDevices, new RoutedEventArgs());
        }

        #region events

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 更新前のデバイスが更新後も存在するなら、そのデバイスを選択したままにする
            var prevSelectedItem = comboBox_Devices.SelectedItem;

            if (comboBox_Devices.ItemsSource == null)
            {
                comboBox_Devices.Items.Clear();
            }
            else
            {
                comboBox_Devices.ItemsSource = null;
            }

            var source = GetDevicesItemSource();
            comboBox_Devices.ItemsSource = source;
            if (source[0] != "-")
            {
                if (comboBox_Devices.Items.Contains(prevSelectedItem))
                {
                    comboBox_Devices.SelectedItem = prevSelectedItem;
                }
                else
                {
                    comboBox_Devices.SelectedIndex = 0;
                }
            }
        }

        private void ComboBox_Devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = e.Source as ComboBox;
            var item = control.SelectedItem == null ? "null" : control.SelectedItem.ToString();
            Console.WriteLine("SelectionChanged: " + item);

            if (((ComboBox)e.Source).SelectedItem != null && dataGrid != null)
            {
                UpdateDataGrid();
            }
        }

        #endregion

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
            var lines = Downloader.GetFiles(Downloader.ScreenshotsDirectoryPath, deviceName);
            for (int i = 0; i < lines.Length; i++)
            {
                Console.WriteLine(lines[i]);
            }
        }

        private void GetVideoshotsTest(string deviceName = "")
        {
            Console.WriteLine("Get videoshots...");
            var lines = Downloader.GetFiles(Downloader.VideoshotsDirectoryPath, deviceName);
            for (int i = 0; i < lines.Length; i++)
            {
                Console.WriteLine(lines[i]);
            }
        }

        private string[] GetDevicesItemSource()
        {
            string[] devices = Downloader.GetDevices();
            string[] source;
            if (devices.Length != 0)
                source = devices;
            else
                source = new string[1] { "-" };
            return source;
        }

        private void UpdateDataGrid()
        {
            string deviceName = comboBox_Devices.SelectedItem.ToString();
            if (deviceName == "-")
                return;

            // ファイル名一覧をDataGrid用に変換
            var array = Downloader.GetAllFiles();
            var data = new Data[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                data[i] = new Data(array[i]);
            }

            // 日時降順ソート
            Array.Sort(data, Data.CompareByDateStringDesc);

            dataGrid.ItemsSource = data;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DownloadSelectedFile();
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key.Equals(Key.Enter)) || (e.Key.Equals(Key.Return)))
            {
                e.Handled = true;

                DownloadSelectedFile();
            }
        }

        private async void DownloadSelectedFile()
        {
            
            string[] files = new string[dataGrid.SelectedItems.Count];

            for (int i = 0; i < dataGrid.SelectedItems.Count; i++)
            {
                var data = (Data)dataGrid.SelectedItems[i];
                files[i] = data.FilePath;
            }

            Console.WriteLine("---------------------------------------------");
            for (int i = 0; i < files.Length; i++)
            {
                Console.WriteLine(string.Format("TargetFiles[{0}]: {1}", i, files[i]));
            }

            Console.WriteLine(string.Format("Start downloading... ({0} files)", files.Length));

            SetControllsInProgress(true);

            await Downloader.DownloadFiles(files);

            SetControllsInProgress(false);

            Console.WriteLine("Download done.");

        }

        private void SetControllsInProgress(bool inProgress)
        {
            this.comboBox_Devices.IsEnabled = !inProgress;
            this.button_UpdateDevices.IsEnabled = !inProgress;
            this.dataGrid.IsEnabled = !inProgress;

            this.progressBar.IsEnabled = inProgress;
            this.progressBar.IsIndeterminate = inProgress;
        }

    }
}
