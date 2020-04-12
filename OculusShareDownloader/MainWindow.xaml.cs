using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;

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
            this.textBox_Path.Text = Downloader.DefaultSaveDirectoryPath;

            Button_Click(button_UpdateDevices, new RoutedEventArgs());
        }

        #region events

        /// <summary>
        /// 更新ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            else
            {
                comboBox_Devices.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 参照ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog("ファイルを保存するフォルダを選択");
            dialog.Multiselect = false;
            dialog.DefaultDirectory = Downloader.DefaulFolderSelectDirectry;
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            // 選択したフォルダパスを表示
            this.textBox_Path.Text = dialog.FileName.ReplaceSeparatorChar(replaceToSlash: true);
            this.textBox_Path.Focus();
            this.textBox_Path.Select(textBox_Path.Text.Length, 0);
        }

        /// <summary>
        /// デバイス一覧コンボボックス選択変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_Devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
#if DEBUG
            var control = e.Source as ComboBox;
            var item = control.SelectedItem == null ? "null" : control.SelectedItem.ToString();
            Console.WriteLine("SelectionChanged: " + item);
#endif
            if (((ComboBox)e.Source).SelectedItem != null && dataGrid != null)
            {
                UpdateDataGrid();
            }
        }

        /// <summary>
        /// DataGridダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGrid.SelectedItems.Count != 0)
                DownloadSelectedFile();
        }

        /// <summary>
        /// キー押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key.Equals(Key.Enter)) || (e.Key.Equals(Key.Return)))
            {
                e.Handled = true; // CurrnetCell移動防止

                if (dataGrid.SelectedItems.Count != 0)
                    DownloadSelectedFile();
            }
        }

#endregion

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

            this.label_Message.Content = string.Empty;
        }

        private async void DownloadSelectedFile()
        {
            string[] files = new string[dataGrid.SelectedItems.Count];
            for (int i = 0; i < dataGrid.SelectedItems.Count; i++)
            {
                var data = (Data)dataGrid.SelectedItems[i];
                files[i] = data.FilePath;
            }
#if DEBUG
            Console.WriteLine("---------------------------------------------");
            for (int i = 0; i < files.Length; i++)
            {
                Console.WriteLine(string.Format("Targets[{0}]: {1}", i, files[i]));
            }
            Console.WriteLine(string.Format("Start file transfer... ({0} files)", files.Length));
#endif

            this.progressBar.Maximum = files.Length;
            this.progressBar.Value = 0;
            var progress = new Progress<int>(ShowProgress);

            string saveDir = this.textBox_Path.Text;

            try
            {
                FreezeControls(true);

                if (!Directory.Exists(saveDir))
                {
#if DEBUG
                    Console.WriteLine(string.Format("フォルダ[{0}]は存在しません。作成します。", saveDir));
#endif
                    Directory.CreateDirectory(saveDir);
                }

                await Downloader.DownloadFiles(progress, files, saveDir);

                this.label_Message.Content = string.Format("{0} 個のファイル転送が完了しました。", files.Length);
            }
            catch(Exception ex)
            {
                this.label_Message.Content = string.Format("ERROR: {0}", ex);
#if DEBUG
                Console.WriteLine(ex);
#endif
                return;
            }
            finally
            {
                FreezeControls(false);
            }

            // 保存先を開く
            string path = $"{saveDir}/{Path.GetFileName(files[0])}";
            Downloader.OpenDirectory(path);

#if DEBUG
            Console.WriteLine("Completed.");
#endif
        }

        /// <summary>
        /// 処理中に触ってほしくないコントロールを無効にする
        /// </summary>
        /// <param name="inProgress"></param>
        private void FreezeControls(bool inProgress)
        {
            this.comboBox_Devices.IsEnabled = !inProgress;
            this.button_UpdateDevices.IsEnabled = !inProgress;
            this.dataGrid.IsEnabled = !inProgress;
            this.button_Browse.IsEnabled = !inProgress;
            this.textBox_Path.IsEnabled = !inProgress;

            this.progressBar.IsEnabled = inProgress;
            
            if (!inProgress)
            {
                this.progressBar.Value = 0;
            }
        }

        /// <summary>
        /// 進捗表示の更新
        /// </summary>
        /// <param name="done"></param>
        private void ShowProgress(int done)
        {
            this.label_Message.Content = string.Format("ファイルを転送中... ({0} / {1})", done, this.progressBar.Maximum);
            this.progressBar.Value = done;
        }
    }
}
