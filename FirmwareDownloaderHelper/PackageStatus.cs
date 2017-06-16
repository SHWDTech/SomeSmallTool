namespace FirmwareDownloaderHelper
{
    public enum PackageStatus : byte
    {
        Unpackaged = 0x00,

        InvalidHead = 0x01,

        BufferHaveNoEnoughLength = 0x02,

        InvalidTail = 0x03,

        CrcCheckFaild = 0x04,

        ComponentError = 0x05,

        DecodeCompleted = 0xFF
    }
}
