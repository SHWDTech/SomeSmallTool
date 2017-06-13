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

namespace WDTech_Firmware_Serial_Loader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ObservableCollection<CheckedBinFile> _checkedBinFiles = new ObservableCollection<CheckedBinFile>();

        public MainWindow()
        {
            InitializeComponent();
            LbLoadedBinFile.ItemsSource = _checkedBinFiles;
            LoadParams();
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
                    if (_checkedBinFiles.Any(f => f.BinFileName == dialog.FileName))
                    {
                        LblMessage.Content = $"文件重复加载：{dialog.FileName}";
                        return;
                    }
                    if (BinFileInfomation.TryParse(binaryReader, out BinFileInfomation info))
                    {
                        TxtSelectBinFileName.Text = dialog.FileName;
                        LblMessage.Content = "BIN文件已选择。";
                        var tabItem = new TabItem
                        {
                            Header = Path.GetFileNameWithoutExtension(dialog.FileName),
                            Content = new BinViewer(),
                            DataContext = info
                        };
                        SelectedBinFileTabControl.Items.Add(tabItem);
                        SelectedBinFileTabControl.SelectedItem = tabItem;
                        _checkedBinFiles.Add(new CheckedBinFile
                        {
                            BinFileName = dialog.FileName
                        });
                    }
                    else
                    {
                        LblMessage.Content = @"BIN文件解析失败，选择了错误的文件，或已经被破坏。";
                    }
                }
            }

            LblMessage.Content = $"文件已选择：{dialog.FileName}";
        }

        private void ClearSelectedFiles(object sender, RoutedEventArgs e)
        {
            SelectedBinFileTabControl.Items.Clear();
            LblMessage.Content = @"清空已选文件。";
        }
    }
}
