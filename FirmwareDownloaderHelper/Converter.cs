using System;

namespace FirmwareDownloaderHelper
{
    public static class Converter
    {
        public static byte[] GetReversedUShortBytes(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);

            return bytes;
        }

        public static byte[] GetReversedUShortBytes(string str)
        {
            var value = ushort.Parse(str);
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);

            return bytes;
        }
    }
}
