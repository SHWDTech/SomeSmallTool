using System;

namespace FirmwareDownloaderHelper.DownloadSender
{
    public class DownloadProcessControl
    {
        public int TotalFileDownloadMissions { get; }

        public int CurrentMission { get; private set; }

        private int _processedMissions;

        private readonly PackageHelper[] _packageHelpers;

        private PackageHelper _onProcessHelper;

        public event ProcessInterrupted ProcessInterrupted;

        public event ProcessFinished ProcessFinished;

        public string CurrentProcessFile => _onProcessHelper == null ? "N/A" : _onProcessHelper.BinFileFullPathWithName;

        public DateTime? LastSendDateTime => _onProcessHelper?.LastSendDateTime;

        public DateTime? LastReceiveDateTime => _onProcessHelper?.LastReceiveDateTime;

        public int? TotalSendByteCount => _onProcessHelper?.TotalSendByteCount;

        public int? TotalReceiveByteCount => _onProcessHelper?.TotalReceiveByteCount;

        public int? LastSendByteCount => _onProcessHelper?.LastSendByteCount;

        public int? LastReceiveByteCount => _onProcessHelper?.LastReceiveByteCount;

        public double? DownloadProgress => _onProcessHelper?.DownloadProgress;

        public double? TotalProgress => (double)_processedMissions / TotalFileDownloadMissions * 100;

        public DownloadProcessControl(BinInfo[] binfileInfos, IDownloadSender downloadSender)
        {
            TotalFileDownloadMissions = binfileInfos.Length;
            _packageHelpers = new PackageHelper[binfileInfos.Length];
            for (var i = 0; i < binfileInfos.Length; i++)
            {
                _packageHelpers[i] = new PackageHelper(binfileInfos[i], downloadSender);
            }
        }

        public void StartProcess()
        {
            if (CurrentMission >= TotalFileDownloadMissions)
            {
                ProcessFinished?.Invoke(new DownloadProcessControlEventArgs
                {
                    Message = $"BIN文件全部下载结束，共{TotalFileDownloadMissions}个文件，成功下载{CurrentMission}个文件。"
                });
                return;
            }
            _onProcessHelper = _packageHelpers[CurrentMission];
            _onProcessHelper.DownloadInterrupted += (e) =>
            {
                _onProcessHelper = null;
                ProcessInterrupted?.Invoke(new DownloadProcessControlEventArgs
                {
                    Message = e.Message,
                    Exception = e.Exception
                });
            };
            _onProcessHelper.DownloadFinished += (e) =>
            {
                StartProcess();
                _processedMissions++;
            };
            _onProcessHelper.StartDownload();
            CurrentMission++;
        }
    }
}
