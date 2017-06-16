using System;
using System.Collections.Generic;

namespace FirmwareDownloaderHelper
{
    public class Package
    {
        protected uint PayloadLength;

        protected ushort BasicLength;

        public PackageStatus PackageStatus { get; protected set; } = PackageStatus.Unpackaged;

        internal Package()
        {            
        }

        public byte ProtocolHead { get; protected set; }

        public byte CommandType { get; protected set; }

        public byte CommandByte { get; protected set; }

        public byte OperateCode { get; protected set; }

        public byte[] RequestCode { get; protected set; }

        public byte[] PayLoadDataLength { get; protected set; }

        public byte[] PayloadData { get; protected set; }

        public byte[] CrcCheck { get; protected set; }

        public byte ProtocolTail { get; protected set; }

        public virtual byte[] EncodeFrame()
        {
            var frame = new List<byte> {ProtocolHead, CommandType, CommandByte, OperateCode};
            frame.AddRange(RequestCode);
            frame.AddRange(PayLoadDataLength);
            frame.AddRange(PayloadData);
            var checkSum = CheckSum.GetUsmbcrc16(frame, frame.Count);
            var crcCheckBytes = BitConverter.GetBytes(checkSum);
            Array.Reverse(crcCheckBytes);
            CrcCheck = crcCheckBytes;
            frame.AddRange(CrcCheck);
            frame.Add(ProtocolTail);

            return frame.ToArray();
        }

        public virtual void DecodeFrame(byte[] buffer)
        {

        }
    }
}
