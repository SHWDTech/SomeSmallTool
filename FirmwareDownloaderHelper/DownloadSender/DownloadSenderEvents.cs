using System;

namespace FirmwareDownloaderHelper.DownloadSender
{

    public delegate void SendSuccess(DownloadSenderSendEventArgs e);

    public delegate void SendFailed(DownloadSenderSendEventArgs e);

    public delegate void Received(DownloadSenderReceivedArgs e);

    public class DownloadSenderEventArgs : EventArgs
    {

    }

    public class DownloadSenderSendEventArgs : DownloadSenderEventArgs
    {
        public byte[] SendContent { get; set; }

        public Exception Exception { get; set; }
    }

    public class DownloadSenderReceivedArgs : DownloadSenderEventArgs
    {
        public byte[] ReceiveContent { get; set; }

        public FirmwareUpdatePackage Package { get; set; }

        public Exception Exception { get; set; }
    }
}
