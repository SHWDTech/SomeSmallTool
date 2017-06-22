using System;
using FirmwareDownloaderHelper.DownloadSender;
using System.Timers;
using WDTech_Firmware_Serial_Loader.Extensions;

namespace FirmwareDownloaderHelper
{
    public class PackageHelper
    {
        private readonly FirmwareUpdatePackage _package;

        public IDownloadSender DownloadSender { get; }

        private readonly BinInfo _binInfo;

        private bool _isDownloading;

        private readonly Timer _timerOutTimer;

        public event DownloadInterrupted DownloadInterrupted;

        public event DownloadFinished DownloadFinished;

        public string BinFileFullPathWithName => _binInfo.BinFileFullPathWithName;

        public DateTime LastSendDateTime { get; private set; }

        public DateTime LastReceiveDateTime { get; private set; }

        public int TotalSendByteCount { get; private set; }

        public int TotalReceiveByteCount { get; private set; }

        public int LastSendByteCount { get; private set; }

        public int LastReceiveByteCount { get; private set; }

        public double DownloadProgress { get; private set; }

        public PackageHelper(BinInfo info, IDownloadSender downloadSender)
        {
            _binInfo = info;
            DownloadSender = downloadSender;
            var rest = info.BinFileLength % info.PackageBinLength;
            var fullPackageCount = info.BinFileLength / info.PackageBinLength;
            var totalPackageCount = rest == 0 ? (ushort)fullPackageCount : (ushort)(fullPackageCount + 1);
            _package = new FirmwareUpdatePackage(totalPackageCount, info.BinFileLength, info.TimeOut);
            _timerOutTimer = new Timer
            {
                Interval = _binInfo.TimeOut * 1000,
                Enabled = true
            };
            _timerOutTimer.Elapsed += (sender, args) =>
            {
                _isDownloading = false;
                DownloadSender.Received -= Received;
                DownloadInterrupt(new DownloadInterruptedEventArgs
                {
                    Message = "等待回复超时，下载中断。"
                });
                _timerOutTimer.Stop();
            };
        }

        public byte[] Pop()
        {
            var startIndex = _package.CurrentIndex * _binInfo.PackageBinLength;
            var remainbyteLength = _binInfo.BinFileBytes.Length - startIndex;
            if (remainbyteLength <= 0)
            {
                return null;
            }
            var binLength = remainbyteLength > _binInfo.PackageBinLength ? _binInfo.PackageBinLength : remainbyteLength;
            var binfileContent = _binInfo.BinFileBytes.SubArray(startIndex, binLength);
            DownloadProgress = (double)startIndex / _binInfo.BinFileLength * 100.0;
            return _package.NextPackage(binfileContent);
        }

        public void StartDownload()
        {
            DownloadSender.SendSuccessed += SendSuccessed;
            DownloadSender.SendFailed += SendFailed;
            DownloadSender.Received += Received;
            _isDownloading = true;

            SendAndResetTimeOut();
        }

        private void SendAndResetTimeOut()
        {
            var sendBytes = Pop();
            if (Pop() == null)
            {
                DownloadFinish(new DownloadFinishedEventArgs
                {
                    Message = @"升级文件下载完成。"
                });
                return;
            }
            ResetTimeOut();
            DownloadSender.Send(sendBytes);
        }

        private void SendSuccessed(DownloadSenderSendEventArgs e)
        {
            LastSendDateTime = DateTime.Now;
            LastSendByteCount = e.SendContent.Length;
            TotalSendByteCount += LastSendByteCount;
            ResetTimeOut();
        }

        private void ResetTimeOut()
        {
            _timerOutTimer.Stop();
            _timerOutTimer.Start();
        }

        private void SendFailed(DownloadSenderSendEventArgs e)
        {
            _isDownloading = false;
            _timerOutTimer.Stop();
            DownloadInterrupt(new DownloadInterruptedEventArgs
            {
                Message = "发送数据失败，下载已中断，请检查串口连接。",
                Exception = e.Exception
            });
        }

        private void Received(DownloadSenderReceivedArgs e)
        {
            if (!_isDownloading) return;
            LastReceiveDateTime = DateTime.Now;
            LastReceiveByteCount = e.ReceiveContent.Length;
            TotalReceiveByteCount += LastReceiveByteCount;
            SendAndResetTimeOut();
            //if (e.Package.PackageStatus == PackageStatus.DecodeCompleted)
            //{
            //    SendAndResetTimeOut();
            //}
            //else if (e.Package.PackageStatus != PackageStatus.BufferHaveNoEnoughLength)
            //{
            //    DownloadInterrupt(new DownloadInterruptedEventArgs
            //    {
            //        Message = "接收到错误的协议包数据，下载已中断。"
            //    });
            //}
        }

        private void DownloadInterrupt(DownloadInterruptedEventArgs e)
        {
            StopPackageDownload();
            DownloadInterrupted?.Invoke(e);
        }

        private void DownloadFinish(DownloadFinishedEventArgs e)
        {
            StopPackageDownload();
            DownloadFinished?.Invoke(e);
        }

        private void StopPackageDownload()
        {
            _isDownloading = false;
            _timerOutTimer.Stop();
            DownloadSender.SendSuccessed -= SendSuccessed;
            DownloadSender.SendFailed -= SendFailed;
            DownloadSender.Received -= Received;
        }
    }
}
