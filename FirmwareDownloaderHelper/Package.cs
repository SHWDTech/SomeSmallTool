using System.Collections.Generic;

namespace FirmwareDownloaderHelper
{
    public class Package
    {
        internal Package()
        {            
        }

        public byte ProtocolHead { get; protected set; }

        public byte CommandType { get; protected set; }

        public byte CommandByte { get; protected set; }

        public byte OperateCode { get; protected set; }

        public byte[] RequestCode { get; protected set; }

        public byte[] PayLoadLength { get; protected set; }

        public byte[] PayloadData { get; protected set; }

        public byte[] CrcCheck { get; protected set; }

        public byte ProtocolTail { get; protected set; }

        public List<byte> PackageBytes { get; protected set; } = new List<byte>();

        protected virtual byte[] CombinePackage()
        {
            PackageBytes.Add(ProtocolHead);
            PackageBytes.Add(CommandType);
            PackageBytes.Add(CommandByte);
            PackageBytes.Add(OperateCode);
            PackageBytes.AddRange(RequestCode);
            PackageBytes.AddRange(PayLoadLength);
            PackageBytes.AddRange(PayloadData);

            CrcCheck = Converter.GetReversedUShortBytes(CheckSum.GetUsmbcrc16(PackageBytes, PackageBytes.Count));
            PackageBytes.AddRange(CrcCheck);
            PackageBytes.Add(ProtocolTail);

            return PackageBytes.ToArray();
        }
    }
}
