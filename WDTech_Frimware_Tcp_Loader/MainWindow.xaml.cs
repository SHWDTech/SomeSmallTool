using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using FirmwareDownloaderHelper;
using FirmwareDownloaderHelper.DownloadSender;
using Microsoft.Win32;
using WDTech_Frimware_Tcp_Loader.Data;
using WDTech_Frimware_Tcp_Loader.Models;
using WDTech_Frimware_Tcp_Loader.UserControl;
using WDTech_Frimware_Tcp_Loader.Views;

namespace WDTech_Frimware_Tcp_Loader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ObservableCollection<CheckedBinFile> _loadedBinFiles = new ObservableCollection<CheckedBinFile>();

        public MainWindow()
        {
            InitializeComponent();
            LbLoadedBinFile.ItemsSource = _loadedBinFiles;
        }

        private void AddNewBinFile(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "bin Files(*.bin)|*.bin|All files(*.*)|*.*",
                Multiselect = false
            };
            var result = dialog.ShowDialog(this);
            if (result.Value)
            {
                using (var binaryReader = new BinaryReader(File.OpenRead(dialog.FileName)))
                {
                    if (_loadedBinFiles.Any(f => f.BinFileName == dialog.FileName))
                    {
                        LblMessage.Content = $"文件重复加载：{dialog.FileName}";
                        return;
                    }
                    if (BinFileInfomation.TryParse(binaryReader, out BinFileInfomation info))
                    {
                        TxtSelectBinFileName.Text = dialog.FileName;
                        LblMessage.Content = "BIN文件已选择。";
                        info.FilePath = dialog.FileName;
                        var tabItem = new TabItem
                        {
                            Header = Path.GetFileNameWithoutExtension(dialog.FileName),
                            Content = new BinViewer(),
                            DataContext = info
                        };
                        SelectedBinFileTabControl.Items.Add(tabItem);
                        SelectedBinFileTabControl.SelectedItem = tabItem;
                        _loadedBinFiles.Add(new CheckedBinFile
                        {
                            BinFileName = dialog.FileName
                        });
                    }
                    else
                    {
                        LblMessage.Content = @"BIN文件解析失败，选择了错误的文件，或已经被破坏。";
                        return;
                    }
                }
            }

            LblMessage.Content = $"文件已选择：{dialog.FileName}";
        }

        private void ClearSelectedFiles(object sender, RoutedEventArgs e)
        {
            SelectedBinFileTabControl.Items.Clear();
            _loadedBinFiles.Clear();
            TxtSelectBinFileName.Text = string.Empty;
            LblMessage.Content = @"清空已选文件。";
        }

        private void SendCurrentBinFile(object sender, RoutedEventArgs e)
        {
            var binViewer = SelectedBinFileTabControl.SelectedContent;
            if (binViewer == null)
            {
                LblMessage.Content = @"当前没有选中BIN文件";
                return;
            }
            var binFileInfo = (BinFileInfomation)((BinViewer)binViewer).DataContext;
            var processControl = GetDownloadProcesser(new[] { binFileInfo });
            StartDownloadProcess(processControl);
        }

        private void SendSelectedBinFile(object sender, RoutedEventArgs e)
        {
            var selectedFileNames = (from object item in LbLoadedBinFile.Items
                    where item is CheckedBinFile
                    select item as CheckedBinFile)
                .Where(f => f.IsChecked).Select(fi => fi.BinFileName).ToList();
            var fileInfos = (from object item in SelectedBinFileTabControl.Items
                    where item is TabItem
                    select (BinFileInfomation)((TabItem)item).DataContext)
                .Where(v => selectedFileNames.Contains(v.FilePath)).ToArray();
            var processControl = GetDownloadProcesser(fileInfos);
            StartDownloadProcess(processControl);
        }

        private void StartDownloadProcess(DownloadProcessControl control)
        {
            var updateTimer = new Timer
            {
                Interval = 100,
                Enabled = true
            };
            updateTimer.Elapsed += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    LblCurrentDownloadFile.Content = control.CurrentProcessFile;
                    LblFileNeedTobeDownload.Content = control.TotalFileDownloadMissions;
                    LblCurrentFileIndex.Content = control.CurrentMission;
                    LblLastSendTime.Content = control.LastSendDateTime == null ? "N/A" : $"{control.LastSendDateTime: HH:mm:ss fff}";
                    LblLastReceiveTime.Content = control.LastReceiveDateTime == null ? "N/A" : $"{control.LastReceiveDateTime: HH:mm:ss fff}";
                    LblTotalSendByteCount.Content = control.TotalSendByteCount == null ? "N/A" : $"{control.TotalSendByteCount}";
                    LblTotalReceiveByteCount.Content = control.TotalReceiveByteCount == null ? "N/A" : $"{control.TotalReceiveByteCount}";
                    LblLastSendByteCount.Content = control.LastSendByteCount == null ? "N/A" : $"{control.LastSendByteCount}";
                    LblLastReceiveByteCount.Content = control.LastReceiveByteCount == null ? "N/A" : $"{control.LastReceiveByteCount}";
                    BarCurrentDownloadProgress.Value = control.DownloadProgress ?? 0;
                    BarTotalDownloadProgress.Value = control.TotalProgress ?? 0;
                });
            };
            updateTimer.Start();
            BtnStartServer.IsEnabled = false;
            control.ProcessFinished += (e) =>
            {
                updateTimer.Stop();
                updateTimer.Dispose();
                Dispatcher.Invoke(() =>
                {
                    BarTotalDownloadProgress.Value = 100;
                    LblMessage.Content = @"下载完成。";
                    MessageBox.Show(e.Message, "系统信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    BtnStartServer.IsEnabled = true;
                });
            };
            control.ProcessInterrupted += (e) =>
            {
                updateTimer.Stop();
                updateTimer.Dispose();
                Dispatcher.Invoke(() =>
                {
                    LblMessage.Content = @"下载已中断。";
                    MessageBox.Show(e.Message, "系统信息", MessageBoxButton.OK, MessageBoxImage.Warning);
                    BtnStartServer.IsEnabled = true;
                });
            };

            control.StartProcess();
            LblMessage.Content = @"开始文件下载。";
        }

        private static DownloadProcessControl GetDownloadProcesser(BinFileInfomation[] infos)
        {
            var binInfos = infos.Select(i => new BinInfo
            {
                BinFileFullPathWithName = i.FilePath,
                BinFileBytes = File.ReadAllBytes(i.FilePath),
                BinFileLength = (uint)new FileInfo(i.FilePath).Length,
                PackageBinLength = DownloadConfigs.PackageBinFileLength,
                TimeOut = DownloadConfigs.TimeOut
            }).ToArray();

            return new DownloadProcessControl(binInfos, new List<IDownloadSender>());
        }

        private void OpenDownloadSetting(object sender, RoutedEventArgs e) => new DownloadSetting
        {
            Owner = this
        }.ShowDialog();
    }
}
