using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SomeSmallTool.Model;
using SomeSmallTool.Process;

namespace SomeSmallTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Timer _autoSendTimer;

        public MainWindow()
        {
            InitializeComponent();
            _autoSendTimer = new Timer { Interval = int.Parse(TxtSendInterval.Text) };
            _autoSendTimer.Elapsed += AutoSendPackage;
            LoadParams();
        }

        private void LoadParams()
        {
            CmbBoundRate.ItemsSource = SerialPortHelper.GetSerialPortBoundRate();
            CmbDataBit.ItemsSource = SerialPortHelper.GetSerialPortDataBits();
            CmbStopBit.ItemsSource = SerialPortHelper.GetSerialPortStopBits();
            CmbParity.ItemsSource = SerialPortHelper.GetSerialPortParity();
            CmbBoundRate.SelectedIndex = 0;
            CmbDataBit.SelectedIndex = 0;
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

            LblPortStatus.Content = @"系统哥哥说，他就知道这几个家伙了。";
        }

        private void OpenCloseSerialPort(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CmbComPort.Text))
            {
                LblPortStatus.Content = "知道什么叫打开串口吗？要先有串口，懂吗？";
                return;
            }
            if (!SerialPortHelper.IsSerialPortOpened())
            {
                SerialPortHelper.OpenSerialPort(CmbComPort.Text,
                    ((BaundRateSelectItem) CmbBoundRate.SelectedItem).Value,
                    ((DataBitSelectItem) CmbDataBit.SelectedItem).Value,
                    ((StopBitSelectItem) CmbStopBit.SelectedItem).Value,
                    ((ParitySelectItem) CmbParity.SelectionBoxItem).Value);
            }
            else
            {
                SerialPortHelper.CloseSerialPort();
            }
            LblPortStatus.Content = SerialPortHelper.GetOperateMessage();
        }

        private void SelectBinFile(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() != true) return;
            TxtSelectedFile.Text = dialog.FileName;
            PrepareSendPackage();
        }

        private void ReFreshSendPackage(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtSelectedFile.Text)) return;
            PrepareSendPackage();
        }

        private void PrepareSendPackage()
        {
            BinFileHelper.PrepareFile(TxtSelectedFile.Text);
            BinFileHelper.SetReadLength(int.Parse(TxtReadLength.Text));
            BinFileHelper.SetFixBytes(GetPrefixBytes(), GetTailfixBytes());
            BinFileHelper.PrepareNextBytes();
            DisplayPreparedPackageString();
        }

        private void OnLittleSisterNumberInput(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            if (!int.TryParse(textBox.Text, out int _))
            {
                textBox.Text = "0";
            }
        }

        private byte[] GetPrefixBytes()
        {
            var prefixText = TxtPrefix.Text.Replace(" ", string.Empty).Trim();
            if (prefixText.Length % 2 != 0) prefixText.Remove(prefixText.Length);
            return StringToByteArray(prefixText);
        }

        private byte[] GetTailfixBytes()
        {
            var tailfixText = TxtTailfix.Text.Replace(" ", string.Empty).Trim();
            if (tailfixText.Length % 2 != 0) tailfixText.Remove(tailfixText.Length);
            return StringToByteArray(tailfixText);
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        private void DisplayPreparedPackageString()
        {
            TxtPreNextPackage.Text = BinFileHelper.GetPreparedString();
        }

        private void SendPreparedBytes()
        {
            if (!BinFileHelper.SendBytesPrepared)
            {
                LblMessages.Content = $"小姐姐们还没准备好！事情是这样的：{BinFileHelper.GetOperateMessage()}。时间：{DateTime.Now:HH:mm:ss fff}";
                return;
            }
            if (!SerialPortHelper.SendBytes(BinFileHelper.GetPreparedBytes()))
            {
                LblMessages.Content = $"发送失败了，你猜是为什么？消息是这样的：{SerialPortHelper.LastException()}。时间：{DateTime.Now:HH:mm:ss fff}";
                return;
            }
            TxtSendBytes.AppendText(BinFileHelper.GetPreparedString() + "\r\n\r\n");
            TxtSendBytes.ScrollToEnd();
            LblSendedBytesCount.Content = BinFileHelper.CountAlreadySend().ToString();
            LblWaitForSend.Content = BinFileHelper.CountExisted().ToString();
            BinFileHelper.PrepareNextBytes();
            DisplayPreparedPackageString();
        }

        private void DoSendOnce(object sender, RoutedEventArgs e)
        {
            SendPreparedBytes();
        }

        private void AutoSendChecked(object sender, RoutedEventArgs e)
        {
            var autoSendCheckBox = sender as CheckBox;
            if (autoSendCheckBox == null) return;
            if (autoSendCheckBox.IsChecked == true)
            {
                _autoSendTimer.Start();
            }
            else
            {
                _autoSendTimer.Stop();
            }
        }

        private void AutoSendPackage(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Dispatcher.Invoke(SendPreparedBytes);
        }
    }
}
