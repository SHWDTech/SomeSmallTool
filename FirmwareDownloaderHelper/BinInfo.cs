namespace FirmwareDownloaderHelper
{
    public class BinInfo
    {
        public string BinFileFullPathWithName { get; set; }

        public ushort PackageBinLength { get; set; }

        public uint BinFileLength { get; set; }

        public ushort TimeOut { get; set; }

        public byte[] BinFileBytes { get; set; }
    }
}
