using System;

namespace FirmwareDownloaderHelper
{
    public delegate void DownloadInterrupted(DownloadInterruptedEventArgs e);

    public delegate void DownloadFinished(DownloadFinishedEventArgs e);

    public class PackageHelperEventArgs : EventArgs
    {
        public string Message { get; set; }

        public Exception Exception { get; set; }
    }

    public class DownloadInterruptedEventArgs : PackageHelperEventArgs
    {
        
    }

    public class DownloadFinishedEventArgs : PackageHelperEventArgs
    {

    }
}
