using System;

namespace WDTech_Firmware_Serial_Loader.Extensions
{
    public static class ArrayExtension
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
