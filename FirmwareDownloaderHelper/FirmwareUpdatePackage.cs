using System;
using System.Collections.Generic;
using WDTech_Firmware_Serial_Loader.Extensions;

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

        public FirmwareUpdatePackage(ushort totalPackageCount, uint binFileByteLegth, ushort timeOut) : this()
        {
            var packageCountBytes = BitConverter.GetBytes(totalPackageCount);
            Array.Reverse(packageCountBytes);
            _totalPackageCount = packageCountBytes;
            var fileLengthBytes = BitConverter.GetBytes(binFileByteLegth);
            Array.Reverse(fileLengthBytes);
            _binfileByteLength = fileLengthBytes;
            var timeOutBtesBytes = BitConverter.GetBytes(timeOut);
            Array.Reverse(timeOutBtesBytes);
            _timeOut = timeOutBtesBytes;
            _sender = 0x01;
            _receiver = 0xFF;
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
            PayloadLength = (uint)(BasicLength + PayloadData.Length);
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
            var payloadDataLenth = buffer[currentIndex] << 24 | buffer[currentIndex + 1] << 16 | buffer[currentIndex + 2] << 8 | buffer[currentIndex + 3];
            if (currentIndex + payloadDataLenth > buffer.Length -3)
            {
                PackageStatus = PackageStatus.BufferHaveNoEnoughLength;
                return;
            }
            currentIndex += 4;
            PayloadData = buffer.SubArray(currentIndex, payloadDataLenth);
            currentIndex += payloadDataLenth;
            CrcCheck = buffer.SubArray(currentIndex, 2);
            if (CrcCheckSum.GetUsmbcrc16(buffer, buffer.Length - 3) != (CrcCheck[0] << 8 | CrcCheck[1]))
            {
                PackageStatus = PackageStatus.CrcCheckFaild;
                return;
            }
            currentIndex++;
            if (buffer[currentIndex] != ProtocolTail)
            {
                PackageStatus = PackageStatus.InvalidTail;
                return;
            }
            PackageStatus = PackageStatus.DecodeCompleted;
        }
    }
}
