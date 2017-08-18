using System;
using System.Collections.Generic;
using System.Text;
using FirmwareDownloaderHelper.Extensions;

namespace FirmwareDownloaderHelper
{
    public class FirmwareUpdatePackage : Package
    {
        private readonly byte _sender;

        private readonly byte _receiver;

        public ushort CurrentIndex { get; private set; }

        private readonly byte[] _totalPackageCount;

        private readonly byte[] _binfileByteLength;

        private readonly byte[] _timeOut;

        private byte? _statusCode;

        public override byte? StatusCode
        {
            get
            {
                if (PackageStatus != PackageStatus.DecodeCompleted) return null;
                return _statusCode;
            }
        }

        public string Description { get; private set; }

        public FirmwareUpdatePackage()
        {
            BasicLength = 11;
            ProtocolHead = 0xAC;
            CommandType = 0xD1;
            CommandByte = 0x02;
            OperateCode = 0x01;
            RequestCode = new byte[] { 0x80, 0x00 };
            ProtocolTail = 0xB1;
        }

        public FirmwareUpdatePackage(ushort totalPackageCount,BinInfo info) : this()
        {
            var packageCountBytes = BitConverter.GetBytes(totalPackageCount);
            Array.Reverse(packageCountBytes);
            _totalPackageCount = packageCountBytes;
            var fileLengthBytes = BitConverter.GetBytes(info.BinFileLength);
            Array.Reverse(fileLengthBytes);
            _binfileByteLength = fileLengthBytes;
            var timeOutBtesBytes = BitConverter.GetBytes(info.TimeOut);
            Array.Reverse(timeOutBtesBytes);
            _timeOut = timeOutBtesBytes;
            _sender = 0x01;
            _receiver = info.TargetObject;
        }

        private void LoadBinFileContent(byte[] binfileContent)
        {
            var data = new List<byte> {_sender, _receiver};
            var currentIndexBytes = BitConverter.GetBytes(CurrentIndex);
            Array.Reverse(currentIndexBytes);
            data.AddRange(currentIndexBytes);
            data.AddRange(_totalPackageCount);
            var binfileDataLength = BitConverter.GetBytes((ushort) binfileContent.Length);
            Array.Reverse(binfileDataLength);
            data.AddRange(binfileDataLength);
            data.AddRange(_binfileByteLength);
            data.AddRange(_timeOut);
            data.AddRange(binfileContent);
            PayloadData = data.ToArray();
            PayloadLength = (ushort)PayloadData.Length;
            var payloaddatalengthBytes = BitConverter.GetBytes(PayloadLength);
            Array.Reverse(payloaddatalengthBytes);
            PayLoadDataLength = payloaddatalengthBytes;

            CurrentIndex++;
        }

        public byte[] NextPackage(byte[] binfileContent)
        {
            LoadBinFileContent(binfileContent);
            return EncodeFrame();
        }

        public override void DecodeFrame(byte[] buffer)
        {
            base.DecodeFrame(buffer);
            if (buffer.Length < BasicLength)
            {
                PackageStatus = PackageStatus.BufferHaveNoEnoughLength;
                return;
            }
            var currentIndex = 0;
            if (buffer[currentIndex] != ProtocolHead)
            {
                PackageStatus = PackageStatus.InvalidHead;
                return;
            }
            currentIndex++;
            if (buffer[currentIndex] != CommandType)
            {
                PackageStatus = PackageStatus.ComponentError;
                return;
            }
            currentIndex++;
            if (buffer[currentIndex] != CommandByte)
            {
                PackageStatus = PackageStatus.ComponentError;
                return;
            }
            currentIndex++;
            OperateCode = buffer[currentIndex];
            currentIndex++;
            if (buffer[currentIndex] != RequestCode[0] || buffer[currentIndex + 1] != RequestCode[1])
            {
                PackageStatus = PackageStatus.ComponentError;
                return;
            }
            currentIndex += 2;
            var payloadDataLenth = buffer[currentIndex] << 8 | buffer[currentIndex + 1];
            currentIndex += 2;
            if (currentIndex + payloadDataLenth + 3 > buffer.Length)
            {
                PackageStatus = PackageStatus.BufferHaveNoEnoughLength;
                return;
            }
            PayloadData = buffer.SubArray(currentIndex, payloadDataLenth);
            currentIndex += payloadDataLenth;
            CrcCheck = buffer.SubArray(currentIndex, 2);
            if (CrcCheckSum.GetUsmbcrc16(buffer, buffer.Length - 3) != (CrcCheck[0] << 8 | CrcCheck[1]))
            {
                PackageStatus = PackageStatus.CrcCheckFaild;
                return;
            }
            currentIndex += 2;
            if (buffer[currentIndex] != ProtocolTail)
            {
                PackageStatus = PackageStatus.InvalidTail;
                return;
            }

            _statusCode = PayloadData[14];
            Description = Encoding.GetEncoding("GBK").GetString(PayloadData, 15, PayloadData.Length - 15);
            PackageStatus = PackageStatus.DecodeCompleted;
        }
    }
}
