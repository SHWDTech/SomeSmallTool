namespace FirmwareDownloaderHelper
{
    public class BinInfo
    {
        public string BinConfigFileFullPathWithName { get; set; }

        public ushort PackageBinLength { get; set; }

        public uint BinFileLength { get; set; }

        public uint BinConfigFileLength { get; set; }

        public ushort TimeOut { get; set; }

        public byte[] BinConfigFileBytes { get; set; }

        public byte TargetObject { get; set; }
    }
}
