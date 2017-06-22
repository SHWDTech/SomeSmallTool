using System;
using System.Linq;
using FirmwareDownloaderHelper.DownloadSender;

namespace FirmwareDownloaderHelper
{
    public class DownloadUnit
    {
        private readonly PackageHelper[] _packageHelpers;

        private PackageHelper _onProcessHelper;

        public int CurrentHelperIndex { get; private set; }

        public string BinFileFullPathWithName => _onProcessHelper == null ? "N/A" : _onProcessHelper.BinFileFullPathWithName;

        public DateTime? LastSendDateTime => _onProcessHelper?.LastSendDateTime;

        public DateTime? LastReceiveDateTime => _onProcessHelper?.LastReceiveDateTime;

        public int? TotalSendByteCount => _packageHelpers.Select(p => p.TotalSendByteCount).Sum();

        public int? TotalReceiveByteCount => _packageHelpers.Select(p => p.TotalReceiveByteCount).Sum();

        public int? LastSendByteCount => _onProcessHelper?.LastSendByteCount;

        public int? LastReceiveByteCount => _onProcessHelper?.LastReceiveByteCount;

        public double? DownloadProgress => _onProcessHelper?.DownloadProgress;

        public int ProcessedFiles { get; private set; }

        public event DownloadInterrupted DownloadInterrupted;

        public event DownloadFinished DownloadFinished;

        public DownloadUnit(BinInfo[] binfileInfos, IDownloadSender downloadSender)
        {
            _packageHelpers = new PackageHelper[binfileInfos.Length];
            var index = 0;
            foreach (var binInfo in binfileInfos)
            {
                _packageHelpers[index] = new PackageHelper(binInfo, downloadSender);
                index++;
            }
        }

        public void StartDownload()
        {
            if (CurrentHelperIndex >= _packageHelpers.Length)
            {
                DownloadFinish(new DownloadFinishedEventArgs
                {
                    Message = "目标设备所有文件下载完成。"
                });
                return;
            }
            _onProcessHelper = _packageHelpers[CurrentHelperIndex];
            _onProcessHelper.DownloadInterrupted += (e) =>
            {
                _onProcessHelper = null;
                DownloadInterrupt(new DownloadInterruptedEventArgs
                {
                    Message = "目标设备下载出现错误，下载中断。"
                });
            };
            _onProcessHelper.DownloadFinished += (e) =>
            {
                StartDownload();
                ProcessedFiles++;
            };
            _onProcessHelper.StartDownload();
            CurrentHelperIndex++;
        }

        private void DownloadInterrupt(DownloadInterruptedEventArgs e)
        {
            DownloadInterrupted?.Invoke(e);
        }

        private void DownloadFinish(DownloadFinishedEventArgs e)
        {
            DownloadFinished?.Invoke(e);
        }
    }
}
