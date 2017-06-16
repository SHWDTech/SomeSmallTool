using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using WDTech_Firmware_Serial_Loader.Models;

namespace WDTech_Firmware_Serial_Loader.Helper
{
    public class SerialPortHelper
    {
        private SerialPort _currentSerialPort;

        private Exception _operateException;

        private string _operateMessage = string.Empty;

        public bool SerialPortIsOpen => _currentSerialPort != null &&_currentSerialPort.IsOpen;

        public bool CanSend => _currentSerialPort.BytesToWrite == 0;

        public event SerialDataReceivedEventHandler DataReceived;

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

            return dataBits.Select(d => new BaundRateSelectItem
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

        public bool OpenSerialPort(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
        {
            try
            {
                if (_currentSerialPort == null)
                {
                    _currentSerialPort = new SerialPort();
                    _currentSerialPort.DataReceived += (sender, e) =>
                    {
                        DataReceived?.Invoke(sender, e);
                    };
                }
                _currentSerialPort.PortName = portName;
                _currentSerialPort.BaudRate = baudRate;
                _currentSerialPort.DataBits = dataBits;
                _currentSerialPort.StopBits = stopBits;
                _currentSerialPort.Parity = parity;
                _currentSerialPort.Open();
                _operateMessage = @"串口已打开。";
            }
            catch (Exception ex)
            {
                _operateException = ex;
                _operateMessage = @"打开串口失败。";
                return false;
            }
            return true;
        }

        public bool CloseSerialPort()
        {
            try
            {
                _currentSerialPort.Close();
                _operateMessage = @"串口已关闭。";
            }
            catch (Exception ex)
            {
                _operateException = ex;
                _operateMessage = @"关闭串口失败";
                return false;
            }
            return true;
        }

        public string GetOperateMessage() => _operateMessage;

        public string LastException() => _operateException?.Message ?? string.Empty;

        public bool IsSerialPortOpened()
        {
            return _currentSerialPort != null && _currentSerialPort.IsOpen;
        }

        public void SendBytes(byte[] bytes) => _currentSerialPort.Write(bytes, 0, bytes.Length);
    }
}
