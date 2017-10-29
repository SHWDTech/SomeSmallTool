using System;
using System.Collections.Generic;
using System.Linq;

namespace FirmwareDownloaderHelper.DownloadSender
{
    public class DownloadProcessControl
    {
        public int TotalFileDownloadMissions { get; }

        public int CurrentFileIndex => _downloadUnints.Select(d => d.CurrentHelperIndex).Sum();

        private readonly DownloadUnit[] _downloadUnints;

        private DownloadUnit _onProcessUnit;

        private int _currentUnitIndex;

        public event ProcessInterrupted ProcessInterrupted;

        public event ProcessFinished ProcessFinished;

        public event ProcessSkiped ProcessSkiped;

        public string CurrentProcessFile => _onProcessUnit == null ? "N/A" : _onProcessUnit.BinFileFullPathWithName;

        public DateTime? LastSendDateTime => _onProcessUnit?.LastSendDateTime;

        public DateTime? LastReceiveDateTime => _onProcessUnit?.LastReceiveDateTime;

        public int? TotalSendByteCount => _downloadUnints.Select(d => d.TotalSendByteCount).Sum();

        public int? TotalReceiveByteCount => _downloadUnints.Select(d => d.TotalReceiveByteCount).Sum();

        public int? LastSendByteCount => _onProcessUnit?.LastSendByteCount;

        public int? LastReceiveByteCount => _onProcessUnit?.LastReceiveByteCount;

        public double? DownloadProgress => _onProcessUnit?.DownloadProgress;

        public double? TotalProgress => (double)_downloadUnints.Select(d => d.ProcessedFiles).Sum() / TotalFileDownloadMissions * 100;

        public DownloadProcessControl(BinInfo[] binfileInfos, IDownloadSender downloadSender)
        {
            TotalFileDownloadMissions = binfileInfos.Length;
            _downloadUnints = new DownloadUnit[1];
            _downloadUnints[0] = new DownloadUnit(binfileInfos, downloadSender);
        }

        public DownloadProcessControl(BinInfo[] binfileInfos, List<IDownloadSender> downloadSenders)
        {
            TotalFileDownloadMissions = binfileInfos.Length * downloadSenders.Count;
            _downloadUnints = new DownloadUnit[downloadSenders.Count];
            var index = 0;
            foreach (var currentDownloadSender in downloadSenders)
            {
                _downloadUnints[index] = new DownloadUnit(binfileInfos, currentDownloadSender);
                index++;
            }
        }

        public void StartProcess()
        {
            if (_currentUnitIndex >= _downloadUnints.Length)
            {
                ProcessFinished?.Invoke(new DownloadProcessControlEventArgs
                {
                    Message = $"BIN文件全部下载结束，共{TotalFileDownloadMissions}个文件，成功下载{CurrentFileIndex}个文件。"
                });
                return;
            }
            _onProcessUnit = _downloadUnints[_currentUnitIndex];
            _onProcessUnit.DownloadInterrupted += (e) =>
            {
                if (_currentUnitIndex >= _downloadUnints.Length)
                {
                    ProcessInterrupted?.Invoke(new DownloadProcessControlEventArgs
                    {
                        Message = e.Message,
                        Exception = e.Exception
                    });
                }
                else
                {
                    ProcessSkiped?.Invoke(new DownloadProcessControlEventArgs
                    {
                        Message = "文件下载出错，下载已跳过，继续处理后续设备下载。",
                        Exception = e.Exception
                    });
                    StartProcess();
                }
            };
            _onProcessUnit.DownloadFinished += (e) => StartProcess();
            _onProcessUnit.StartDownload();
            _currentUnitIndex++;
        }
    }
}
