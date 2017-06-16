using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using FirmwareDownloaderHelper.DownloadSender;
using System.Diagnostics;
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
                var readBytes = Encoding.UTF8.GetBytes(((SerialPort)sender).ReadExisting());
                _buffer.AddRange(readBytes);
                var package = DecodePackage();
                Received?.Invoke(new DownloadSenderReceivedArgs
                {
                    ReceiveContent = readBytes,
                    Package = package
                });
            };
        }

        public void Send(byte[] content)
        {
            try
            {
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
                Debug.Write(ex);
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
            else if (package.PackageStatus != PackageStatus.DecodeCompleted)
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
