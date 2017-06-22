using System;

namespace FirmwareDownloaderHelper.DownloadSender
{
    public delegate void ProcessInterrupted(DownloadProcessControlEventArgs e);

    public delegate void ProcessFinished(DownloadProcessControlEventArgs e);

    public delegate void ProcessSkiped(DownloadProcessControlEventArgs e);

    public class DownloadProcessControlEventArgs : EventArgs
    {
        public string Message { get; set; }

        public Exception Exception { get; set; }
    }
}
