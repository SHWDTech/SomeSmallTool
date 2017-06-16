namespace FirmwareDownloaderHelper.DownloadSender
{
    public interface IDownloadSender
    {
        void Send(byte[] content);

        event SendSuccess SendSuccessed;

        event SendFailed SendFailed;

        event Received Received;
    }
}
