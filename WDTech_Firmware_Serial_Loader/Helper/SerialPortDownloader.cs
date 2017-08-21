using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using FirmwareDownloaderHelper.DownloadSender;
using FirmwareDownloaderHelper;

namespace WDTech_Firmware_Serial_Loader.Helper
{
    public class SerialPortDownloader : IDownloadSender
    {
        private readonly List<byte> _buffer = new List<byte>();

        private readonly SerialPortHelper _portHelper;

        public SerialPortDownloader(SerialPortHelper portHelper)
        {
            _portHelper = portHelper;
            _portHelper.DataReceived += (sender, e) =>
            {
                var port = sender as SerialPort;
                if (port == null) return;
                var readBytes = new List<byte>();
                lock (_buffer)
                {
                    while (port.BytesToRead > 0)
                    {
                        readBytes.Add((byte)port.ReadByte());
                    }
                    _buffer.AddRange(readBytes);
                }
                var package = DecodePackage();
                Received?.Invoke(new DownloadSenderReceivedArgs
                {
                    ReceiveContent = readBytes.ToArray(),
                    Package = package
                });
            };
        }

        public void Send(byte[] content)
        {
            try
            {
                Thread.Sleep(5);
                _portHelper.SendBytes(content);
                SendSuccessed?.Invoke(new DownloadSenderSendEventArgs
                {
                    SendContent = content
                });
            }
            catch (Exception ex)
            {
                SendFailed?.Invoke(new DownloadSenderSendEventArgs
                {
                    SendContent = content,
                    Exception = ex
                });
            }
        }

        private FirmwareUpdatePackage DecodePackage()
        {
            var package = new FirmwareUpdatePackage();
            package.DecodeFrame(_buffer.ToArray());
            if (package.PackageStatus == PackageStatus.InvalidHead)
            {
                if (_buffer.Count > 0)
                {
                    _buffer.RemoveAt(0);
                }
                DecodePackage();
            }
            else if (package.PackageStatus == PackageStatus.BufferHaveNoEnoughLength)
            {
                return package;
            }
            else
            {
                _buffer.Clear();
            }
            return package;
        }

        public event SendSuccess SendSuccessed;

        public event SendFailed SendFailed;

        public event Received Received;
    }
}
