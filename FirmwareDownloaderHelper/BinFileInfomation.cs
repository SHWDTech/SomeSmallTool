using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FirmwareDownloaderHelper
{
    public class BinFileInfomation
    {
        private byte _targetObject;

        private byte[] _binFileSize;

        private byte[] _updateMode;

        private byte[] _firmwareReleaseDate;

        private byte[] _firmwareVersion;

        private byte[] _description;

        private byte[] _binFileCheckSum;

        private byte[] _describeCheckSum;

        private BinFileInfomation()
        {
            
        }

        public string TargetObject => BinFileOptionsHelper.GetUpdateTarget(_targetObject.ToString()); 

        public int BinFileSize
        {
            get
            {
                var fileSizeArray = new byte[_binFileSize.Length];
                Array.Copy(_binFileSize, fileSizeArray, _binFileSize.Length);
                Array.Reverse(fileSizeArray);
                return BitConverter.ToInt32(fileSizeArray, 0);
            }
        }

        public string UpdateMode => BinFileOptionsHelper.GetUpdateMode(BitConverter.ToString(_updateMode));

        public byte FirmwareReleaseYear => _firmwareReleaseDate[0];

        public byte FirmwareReleaseMonth => _firmwareReleaseDate[1];

        public byte FirmwareReleaseDay => _firmwareReleaseDate[2];

        public byte FirmwareReleaseHour => _firmwareReleaseDate[3];

        public byte FirmwareReleaseMinute => _firmwareReleaseDate[4];

        public byte FirmwareReleaseSecond => _firmwareReleaseDate[5];

        public byte FirmwareVersionFirst => _firmwareVersion[0];

        public byte FirmwareVersionSecond => _firmwareVersion[1];

        public byte FirmwareVersionThird => _firmwareVersion[2];

        public byte FirmwareVersionFourth => _firmwareVersion[3];

        public string Description => Encoding.GetEncoding("GBK").GetString(_description).Replace("\0", string.Empty);

        public string BinFileCheckSum => BitConverter.ToString(_binFileCheckSum).Replace('-', ' ');

        public string DescribeCheckSum => BitConverter.ToString(_describeCheckSum).Replace('-', ' ');

        public int DescriptionLength => _description.Length;

        public static bool TryParse(BinaryReader reader, out BinFileInfomation info)
        {
            info = new BinFileInfomation();
            try
            {
                var head = reader.ReadByte();
                info._targetObject = reader.ReadByte();
                info._binFileSize = reader.ReadBytes(4);
                info._updateMode = reader.ReadBytes(2);
                info._firmwareReleaseDate = reader.ReadBytes(6);
                info._firmwareVersion = reader.ReadBytes(4);
                info._description = reader.ReadBytes(233);
                info._binFileCheckSum = reader.ReadBytes(2);
                info._describeCheckSum = reader.ReadBytes(2);
                reader.ReadByte();
                var binfileBytes = reader.ReadBytes((int) (reader.BaseStream.Length - reader.BaseStream.Position));
                var binCheck = BitConverter.GetBytes(CrcCheckSum.GetUsmbcrc16(binfileBytes, binfileBytes.Length));
                Array.Reverse(binCheck);
                if (!info._binFileCheckSum.ToArray().SequenceEqual(binCheck))
                {
                    info = null;
                    return false;
                }
                var descCheck = new List<byte> {head, info._targetObject};
                var binSize = new byte[4];
                info._binFileSize.CopyTo(binSize, 0);
                descCheck.AddRange(binSize);
                descCheck.AddRange(info._updateMode);
                descCheck.AddRange(info._firmwareReleaseDate);
                descCheck.AddRange(info._firmwareVersion);
                descCheck.AddRange(info._description);
                descCheck.AddRange(binCheck);
                var descCheckSum =
                    BitConverter.GetBytes(CrcCheckSum.GetUsmbcrc16(descCheck.ToArray(), descCheck.Count));
                Array.Reverse(descCheckSum);
                if (!info._describeCheckSum.ToArray().SequenceEqual(descCheckSum))
                {
                    info = null;
                    return false;
                }
            }
            catch (Exception)
            {
                info = null;
                return false;
            }
            return true;
        }
    }
}
