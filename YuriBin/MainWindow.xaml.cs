using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using static YuriBin.Models.ComboBoxItemModels;

namespace YuriBin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string _checkMessage;

        private byte[] _finalInfomationBytes;

        private byte[] _binFileBytes;

        public MainWindow()
        {
            InitializeComponent();
            AddComboBoxItems();
            _checkMessage = string.Empty;
        }

        private void AddComboBoxItems()
        {
            CmbTargetObject.Items.Add(new TargetObjectItem
            {
                Name = "107",
                Value = 0x02
            });
            CmbTargetObject.Items.Add(new TargetObjectItem
            {
                Name = "103",
                Value = 0x03
            });
            CmbTargetObject.SelectedIndex = 0;

            CmbUpdateMode.Items.Add(new UpdateModeItem
            {
                Name = "手动更新模式",
                Value = new byte[] { 0x00, 0x00 }
            });
            CmbUpdateMode.Items.Add(new UpdateModeItem
            {
                Name = "老郑想想更新模式",
                Value = new byte[] { 0x00, 0x03 }
            });
            CmbUpdateMode.Items.Add(new UpdateModeItem
            {
                Name = "版本号更新模式",
                Value = new byte[] { 0x00, 0x02 }
            });
            CmbUpdateMode.Items.Add(new UpdateModeItem
            {
                Name = "发布时间更新模式",
                Value = new byte[] { 0x00, 0x01 }
            });
            CmbUpdateMode.Items.Add(new UpdateModeItem
            {
                Name = "强制更新模式",
                Value = new byte[] { 0x00, 0x04 }
            });
            CmbUpdateMode.SelectedIndex = 4;
        }

        private void SelectBinFile(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "bin Files(*.bin)|*.bin|All files(*.*)|*.*",
                Multiselect = false
            };
            var result = dialog.ShowDialog(this);
            if (result.Value)
            {
                TxtSelectBinFileName.Text = dialog.FileName;
                TxtYuriBinSize.Text = new FileInfo(dialog.FileName).Length.ToString();
                LblMessage.Content = "BIN文件已选择。";
            }
        }

        private void CommitCurrentDateTime(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now;
            TxtReleashYear.Text = now.Year.ToString();
            TxtReleashMonth.Text = now.Month.ToString();
            TxtReleashDay.Text = now.Day.ToString();
            TxtReleashHour.Text = now.Hour.ToString();
            TxtReleashMinute.Text = now.Minute.ToString();
            TxtReleashSecond.Text = now.Second.ToString();
        }

        private void CalcCheckSum(object sender, RoutedEventArgs e)
        {
            if (!CheckInfoPrepared())
            {
                MessageBox.Show(_checkMessage);
                e.Handled = true;
                return;
            }

            _binFileBytes = File.ReadAllBytes(TxtSelectBinFileName.Text);
            var binCheckSumBytes = BitConverter.GetBytes(CrcCheckSum.GetUsmbcrc16(_binFileBytes, _binFileBytes.Length));
            Array.Reverse(binCheckSumBytes);
            TxtBinCheck.Text = BitConverter.ToString(binCheckSumBytes).Replace("-", " ");
            var descList = CombineBytes(binCheckSumBytes);
            var descCheckSumBytes = BitConverter.GetBytes(CrcCheckSum.GetUsmbcrc16(descList.ToArray(), descList.Count));
            Array.Reverse(descCheckSumBytes);
            TxtDescCheck.Text = BitConverter.ToString(descCheckSumBytes).Replace("-", " ");
            descList.AddRange(descCheckSumBytes);
            descList.Add(0xB1);
            _finalInfomationBytes = descList.ToArray();

            TxtDescPreview.Clear();
            TxtDescPreview.Text = BitConverter.ToString(_finalInfomationBytes).Replace("-", " ");
            LblMessage.Content = "校验完成，可以生成BIN_CFG文件。";
        }

        private List<byte> CombineBytes(byte[] binCheckSum)
        {
            var container = new List<byte> {0xAC, (byte) CmbTargetObject.SelectedValue};
            var binSize = BitConverter.GetBytes(int.Parse(TxtYuriBinSize.Text));
            Array.Reverse(binSize);
            container.AddRange(binSize);
            container.AddRange((byte[]) CmbUpdateMode.SelectedValue);
            container.Add(byte.Parse(TxtReleashYear.Text.Substring(2, 2)));
            container.Add(byte.Parse(TxtReleashMonth.Text));
            container.Add(byte.Parse(TxtReleashDay.Text));
            container.Add(byte.Parse(TxtReleashHour.Text));
            container.Add(byte.Parse(TxtReleashMinute.Text));
            container.Add(byte.Parse(TxtReleashSecond.Text));
            container.Add(byte.Parse(TxtVersionCodeFirst.Text));
            container.Add(byte.Parse(TxtVersionCodeSecond.Text));
            container.Add(byte.Parse(TxtVersionCodeThird.Text));
            container.Add(byte.Parse(TxtVersionCodeFourth.Text));
            var descStr = TxtDescribe.Text;
            var descBytes = PopDescriptionBytes();
            if (descStr.Length < 232)
            {
                Encoding.GetEncoding("GBK").GetBytes(descStr, 0, descStr.Length, descBytes, 0);
            }
            else if (descStr.Length > 232)
            {
                Encoding.GetEncoding("GBK").GetBytes(descStr, 232, descStr.Length, descBytes, 0);
            }
            container.AddRange(descBytes);
            container.AddRange(binCheckSum);
            return container;
        }

        private bool CheckInfoPrepared()
        {
            if (string.IsNullOrWhiteSpace(TxtSelectBinFileName.Text))
            {
                _checkMessage = "阿卡林还没找到，先找阿卡林吧！";
                LblMessage.Content = "还未选择BIN文件。";
                return false;
            }

            if (!DateTime.TryParse($"{TxtReleashYear.Text}-{TxtReleashMonth.Text}-{TxtReleashDay.Text} {TxtReleashHour.Text}:{TxtReleashMinute.Text}:{TxtReleashSecond.Text}", out DateTime _))
            {
                _checkMessage = "时间不对！打回去重睡！";
                LblMessage.Content = "时间信息有误。";
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtVersionCodeFirst.Text) ||
                string.IsNullOrWhiteSpace(TxtVersionCodeSecond.Text) ||
                string.IsNullOrWhiteSpace(TxtVersionCodeThird.Text) ||
                string.IsNullOrWhiteSpace(TxtVersionCodeFourth.Text))
            {
                _checkMessage = "SA KA MO DO呢？";
                LblMessage.Content = "版本信息有误。";
                return false;
            }

            return true;
        }

        private void OnDescChanged(object sender, RoutedEventArgs e)
        {
            TxtDescribeUsed.Text = $"{TxtDescribe.Text.Length + 1}";
        }

        private void WriteBinCfgFile(object sender, RoutedEventArgs e)
        {
            CalcCheckSum(sender, e);
            if (e.Handled) return;
            var bytes = new List<byte>();
            bytes.AddRange(_finalInfomationBytes);
            bytes.AddRange(_binFileBytes);
            var bincfgFilePath = Path.GetDirectoryName(TxtSelectBinFileName.Text);
            var bincfgFileName = $"{bincfgFilePath}\\{Path.GetFileNameWithoutExtension(TxtSelectBinFileName.Text)}_cfg.bin";
            using (var file = File.Open(bincfgFileName, FileMode.OpenOrCreate))
            {
                file.Write(bytes.ToArray(), 0, bytes.Count);
            }
            LblMessage.Content = "文件写入成功。";
        }

        private void JumpToNext(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Decimal)
            {
                var tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                var keyboardFocus = Keyboard.FocusedElement as UIElement;

                keyboardFocus?.MoveFocus(tRequest);
                e.Handled = true;
            }
        }

        private static byte[] PopDescriptionBytes()
        {
            var desBytes = new byte[233];
            for (var i = 0; i < desBytes.Length; i++)
            {
                desBytes[i] = 0;
            }

            return desBytes;
        }
    }
}
