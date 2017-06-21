﻿using System;
using System.Collections.Generic;
using FirmwareDownloaderHelper;
using FirmwareDownloaderHelper.DownloadSender;

namespace WDTech_Frimware_Tcp_Loader.Helper
{
    public class TcpDownloader : IDownloadSender
    {
        private readonly SocketClient _client;

        private readonly List<byte> _buffer = new List<byte>();

        public TcpDownloader(SocketClient client)
        {
            _client = client;
            _client.TcpDataReceived += (e) =>
            {
                var readBytes = new byte[e.BytesTransferred];
                for (var i = 0; i < e.BytesTransferred; i++)
                {
                    readBytes[i] = e.Buffer[i];
                    _buffer.AddRange(readBytes);
                }
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
                _client.Send(content);
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
