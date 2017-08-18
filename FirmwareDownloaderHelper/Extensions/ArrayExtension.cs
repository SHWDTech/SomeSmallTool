using System;
using System.Text;

namespace FirmwareDownloaderHelper.Extensions
{
    public static class ArrayExtension
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static string ToHexString(this byte[] data)
        {
            var builder = new StringBuilder();
            foreach (var b in data)
            {
                builder.Append($"{b:X2}".PadRight(3, ' '));
            }
            return builder.ToString();
        }
    }
}
