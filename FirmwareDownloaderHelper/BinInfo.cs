namespace FirmwareDownloaderHelper
{
    public class BinInfo
    {
        public string BinFileSender { get; set; }

        public string BinFileReceiver { get; set; }

        public ushort PackageBinLength { get; set; }

        public ushort BinFileLength { get; set; }

        public byte[] BinFileContent { get; set; }

        public string StatusCode { get; set; }

        public string StatusDescrib { get; set; }
    }
}
