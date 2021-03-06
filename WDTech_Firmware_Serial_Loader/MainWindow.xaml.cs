﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using WDTech_Firmware_Serial_Loader.Helper;
using System.Windows.Controls;
using FirmwareDownloaderHelper;
using WDTech_Firmware_Serial_Loader.Data;
using WDTech_Firmware_Serial_Loader.UserControl;
using WDTech_Firmware_Serial_Loader.Views;
using System.IO.Ports;
using System.Timers;
using FirmwareDownloaderHelper.DownloadSender;
using WDTech_Firmware_Serial_Loader.Models;

namespace WDTech_Firmware_Serial_Loader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ObservableCollection<CheckedBinFile> _loadedBinFiles = new ObservableCollection<CheckedBinFile>();

        private readonly SerialPortHelper _portHelper;

        private readonly SerialPortDownloader _portDownloader;

        private string _messageBarMessage = string.Empty;

        private readonly Timer _messageTimer;

        public MainWindow()
        {
            InitializeComponent();
            LbLoadedBinFile.ItemsSource = _loadedBinFiles;
            _messageTimer = new Timer
            {
                Interval = 10,
                Enabled = true
            };
            LoadParams();
            _portHelper = new SerialPortHelper();
            _portDownloader = new SerialPortDownloader(_portHelper);
        }

        private void LoadParams()
        {
            CmbBoundRate.ItemsSource = SerialPortHelper.GetSerialPortBoundRate();
            CmbDataBit.ItemsSource = SerialPortHelper.GetSerialPortDataBits();
            CmbStopBit.ItemsSource = SerialPortHelper.GetSerialPortStopBits();
            CmbParity.ItemsSource = SerialPortHelper.GetSerialPortParity();
            CmbBoundRate.SelectedIndex = 9;
            CmbDataBit.SelectedIndex = 3;
            CmbStopBit.SelectedIndex = 0;
            CmbParity.SelectedIndex = 0;
            RefreshSystemSerialPortList();
            _messageTimer.Elapsed += (sender, args) => Dispatcher.Invoke(() => LblMessage.Content = _messageBarMessage);
            _messageTimer.Start();
        }

        private void ReFindSystemSerialPort(object sender, RoutedEventArgs e) => RefreshSystemSerialPortList();

        private void RefreshSystemSerialPortList()
        {
            CmbComPort.ItemsSource = SerialPortHelper.GetSerialPorts();
            if (CmbComPort.Items.Count > 0)
            {
                CmbComPort.SelectedIndex = 0;
            }
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
                        _messageBarMessage = $"文件重复加载：{dialog.FileName}";
                        return;
                    }
                    if (BinFileInfomation.TryParse(binaryReader, out BinFileInfomation info))
                    {
                        TxtSelectBinFileName.Text = dialog.FileName;
                        _messageBarMessage = "BIN文件已选择。";
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
                        _messageBarMessage = "BIN文件解析失败，选择了错误的文件，或已经被破坏。";
                        return;
                    }
                }
            }

            _messageBarMessage = $"文件已选择：{dialog.FileName}";
        }

        private void ClearSelectedFiles(object sender, RoutedEventArgs e)
        {
            SelectedBinFileTabControl.Items.Clear();
            _loadedBinFiles.Clear();
            TxtSelectBinFileName.Text = string.Empty;
            _messageBarMessage = "清空已选文件。";
        }

        private void SendCurrentBinFile(object sender, RoutedEventArgs e)
        {
            if (!_portHelper.SerialPortIsOpen)
            {
                _messageBarMessage = "串口还没有打开！";
                return;
            }
            var binViewer = SelectedBinFileTabControl.SelectedContent;
            if (binViewer == null)
            {
                _messageBarMessage = "当前没有选中的BIN文件。";
                return;
            }
            var binFileInfo = (BinFileInfomation)((BinViewer)binViewer).DataContext;
            var processControl = GetDownloadProcesser(new[] {binFileInfo});
            StartDownloadProcess(processControl);
        }

        private void SendSelectedBinFile(object sender, RoutedEventArgs e)
        {
            if (!_portHelper.SerialPortIsOpen)
            {
                _messageBarMessage = "串口还没有打开！";
                return;
            }
            var selectedFileNames = (from object item in LbLoadedBinFile.Items
                                     where item is CheckedBinFile
                                     select item as CheckedBinFile)
                .Where(f => f.IsChecked).Select(fi => fi.BinFileName).ToList();
            var fileInfos = (from object item in SelectedBinFileTabControl.Items
                             where item is TabItem
                             select (BinFileInfomation)((TabItem)item).DataContext)
                .Where(v => selectedFileNames.Contains(v.FilePath)).ToArray();
            if (fileInfos.Length <= 0)
            {
                _messageBarMessage = "当前没有选中的BIN文件。";
                return;
            }
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
                    LblCurrentFileIndex.Content = control.CurrentFileIndex;
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
            BtnSwitchSerialPortStatus.IsEnabled = false;
            control.ProcessFinished += (e) =>
            {
                updateTimer.Stop();
                updateTimer.Dispose();
                Dispatcher.Invoke(() =>
                {
                    BarTotalDownloadProgress.Value = BarCurrentDownloadProgress.Value = 100;
                    _messageBarMessage = "下载完成。";
                    MessageBox.Show(e.Message, "系统信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    BtnSwitchSerialPortStatus.IsEnabled = true;
                    UpdateDownloadBtnStatus(true);
                });
            };
            control.ProcessInterrupted += (e) =>
            {
                updateTimer.Stop();
                updateTimer.Dispose();
                Dispatcher.Invoke(() =>
                {
                    _messageBarMessage = "下载已中断。";
                    MessageBox.Show(e.Message, "系统信息", MessageBoxButton.OK, MessageBoxImage.Warning);
                    BtnSwitchSerialPortStatus.IsEnabled = true;
                    UpdateDownloadBtnStatus(true);
                });
            };

            control.StartProcess();
            _messageBarMessage = "开始文件下载.";
            UpdateDownloadBtnStatus(false);
        }

        private void UpdateDownloadBtnStatus(bool status)
        {
            BtnSendCurrentBinFile.IsEnabled = BtnSendSelectedBinFile.IsEnabled = status;
        }

        private DownloadProcessControl GetDownloadProcesser(IEnumerable<BinFileInfomation> infos)
        {
            var binInfos = infos.Select(i => new BinInfo
            {
                BinConfigFileFullPathWithName = i.FilePath,
                BinConfigFileBytes = File.ReadAllBytes(i.FilePath),
                BinConfigFileLength = (uint)new FileInfo(i.FilePath).Length,
                BinFileLength = (uint)i.BinFileSize,
                PackageBinLength = DownloadConfigs.PackageBinFileLength,
                TimeOut = DownloadConfigs.TimeOut,
                TargetObject = i.TargetObject
            }).ToArray();

            return new DownloadProcessControl(binInfos, _portDownloader);
        }

        private void OpenDownloadSetting(object sender, RoutedEventArgs e) => new DownloadSetting
        {
            Owner = this
        }.ShowDialog();

        private void SwitchSerialPortStatus(object sender, RoutedEventArgs e)
        {
            if (!_portHelper.SerialPortIsOpen)
            {
                Dispatcher.Invoke(() =>
                {
                    LockSerialPortParamControls(false);
                    _portHelper.OpenSerialPort(CmbComPort.Text
                        , (int)CmbBoundRate.SelectedValue
                        , (int)CmbDataBit.SelectedValue
                        , (StopBits)CmbStopBit.SelectedValue
                        , (Parity)CmbParity.SelectedValue);
                    if (_portHelper.SerialPortIsOpen)
                    {
                        BtnSwitchSerialPortStatus.Content = "关闭串口";
                        _messageBarMessage = "串口已经打开";
                    }
                    else
                    {
                        LockSerialPortParamControls(true);
                        _messageBarMessage = "串口打开失败，请检查是否被占用";
                    }
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    _portHelper.CloseSerialPort();
                    if (!_portHelper.SerialPortIsOpen)
                    {
                        BtnSwitchSerialPortStatus.Content = "打开串口";
                        LockSerialPortParamControls(true);
                        _messageBarMessage = "串口已经关闭";
                    }
                });
            }
        }

        private void LockSerialPortParamControls(bool status)
        {
            CmbComPort.IsEnabled = CmbBoundRate.IsEnabled =
                CmbDataBit.IsEnabled = CmbStopBit.IsEnabled = CmbParity.IsEnabled = status;
        }
    }
}
