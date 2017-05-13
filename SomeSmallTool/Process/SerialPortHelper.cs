using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using SomeSmallTool.Model;

namespace SomeSmallTool.Process
{
    public class SerialPortHelper
    {
        private static SerialPort _currentSerialPort;

        private static Exception _operateException;

        private static string _operateMessage = string.Empty;

        public static List<StopBitSelectItem> GetSerialPortStopBits()
        {
            var values = Enum.GetValues(typeof(StopBits)).Cast<StopBits>().Where(s => s!= StopBits.None);
            return values.Select(v => new StopBitSelectItem
            {
                Name = StopBitName.GetStopBitName(v.ToString()),
                Value = v
            }).ToList();
        }

        public static List<DataBitSelectItem> GetSerialPortDataBits()
        {
            var dataBits = new[] { 5, 6, 7, 8 };

            return dataBits.Select(d => new DataBitSelectItem
            {
                Name = d.ToString(),
                Value = d
            }).ToList();
        }

        public static List<BaundRateSelectItem> GetSerialPortBoundRate()
        {
            var dataBits = new[] { 1200, 2400, 4800, 9600, 14400, 19200, 38400, 56000, 57600, 115200, 194000 };

            return dataBits.Select(d => new BaundRateSelectItem()
            {
                Name = d.ToString(),
                Value = d
            }).ToList();
        }

        public static List<ParitySelectItem> GetSerialPortParity()
        {
            var values = Enum.GetValues(typeof(Parity)).Cast<Parity>();
            return values.Select(v => new ParitySelectItem
            {
                Name = v.ToString(),
                Value = v
            }).ToList();
        }

        public static List<SerialPortSelectItem> GetSerialPorts()
        {
            var values = SerialPort.GetPortNames();
            return values.Select(v => new SerialPortSelectItem
            {
                Name = v,
                Value = v
            }).ToList();
        }

        public static void OpenSerialPort(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
        {
            try
            {
                _currentSerialPort = new SerialPort
                {
                    PortName = portName,
                    BaudRate = baudRate,
                    DataBits = dataBits,
                    StopBits = stopBits,
                    Parity = parity
                };
                _currentSerialPort.Open();
                _operateMessage = @"行了，打开了，你就干吧。";
            }
            catch (Exception ex)
            {
                _operateException = ex;
                _operateMessage = @"搞事情，你又搞事情！";
            }
        }

        public static void CloseSerialPort()
        {
            try
            {
                _currentSerialPort.Close();
                _operateMessage = @"OY，关掉啦，下班咯！";
            }
            catch (Exception ex)
            {
                _operateException = ex;
                _operateMessage = @"搞事情，你又搞事情！";
            }
        }

        public static string GetOperateMessage() => _operateMessage;

        public static string LastException() => _operateException?.Message ?? string.Empty;

        public static bool IsSerialPortOpened()
        {
            return _currentSerialPort != null && _currentSerialPort.IsOpen;
        }

        public static bool SendBytes(byte[] bytes)
        {
            try
            {
                _currentSerialPort.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                _operateException = ex;
                _operateMessage = @"OH MY GOD，小姐姐们出发失败了！";
                return false;
            }
            return true;
        }
    }
}
