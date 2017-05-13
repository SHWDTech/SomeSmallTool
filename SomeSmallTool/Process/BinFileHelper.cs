using System;
using System.IO;

namespace SomeSmallTool.Process
{
    public class BinFileHelper
    {
        private static BinaryReader _currentReader;

        private static int _readLength;

        private static byte[] _preparedBytes;


        private static byte[] _prefixBytes;

        private static byte[] _tailfixBytes;

        private static string _operateMessage;

        private static Exception _operateException;

        public static void PrepareFile(string fileName)
        {
            _currentReader?.Close();
            _currentReader = new BinaryReader(new FileStream(fileName, FileMode.Open));
        }

        public static void SetReadLength(int readLength) => _readLength = readLength;

        public static void SetFixBytes(byte[] prefixBytes, byte[] tailfixBytes)
        {
            _prefixBytes = prefixBytes;
            _tailfixBytes = tailfixBytes;
        }

        public static bool PrepareNextBytes()
        {
            if (_currentReader.BaseStream.Position + _readLength > _currentReader.BaseStream.Length)
            {
                _operateMessage = "已经到头了！别怼了！";
                return false;
            }
            try
            {
                _preparedBytes = new byte[_prefixBytes.Length + _readLength + _tailfixBytes.Length];
                Array.Copy(_prefixBytes, _preparedBytes, _prefixBytes.Length);
                var fileReadBytes = new byte[_readLength];
                _currentReader.Read(fileReadBytes, 0, _readLength);
                Array.Copy(fileReadBytes, 0, _preparedBytes, _prefixBytes.Length, fileReadBytes.Length);
                Array.Copy(_tailfixBytes, 0, _preparedBytes, _prefixBytes.Length + _readLength, _tailfixBytes.Length);
                return true;
            }
            catch (Exception ex)
            {
                _operateException = ex;
                _operateMessage = "读小姐姐们的时候出事了！";
                return false;
            }
        }

        public static string GetOperateMessage() => _operateMessage;

        public static string LastException() => _operateException?.Message ?? string.Empty;

        public static string GetPreparedString()
        {
            return _preparedBytes == null ? string.Empty : BitConverter.ToString(_preparedBytes).Replace("-", " ");
        }

        public static byte[] GetPreparedBytes() => _preparedBytes;
    }
}
