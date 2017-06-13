using System.Collections.Generic;

namespace FirmwareDownloaderHelper
{
    public class FirmwareUpdatePackage : Package
    {
        private ushort _currentIndex;

        private FirmwareUpdatePackage()
        {
            ProtocolHead = 0xAC;
            CommandType = 0xD1;
            CommandByte = 0x02;
            OperateCode = 0x01;
            RequestCode = new byte[] {0x80, 0x00};
            ProtocolTail = 0xB1;
        }

        public FirmwareUpdatePackage(BinInfo info) : this()
        {
            var payLoadDataByteList = new List<byte>
            {
                byte.Parse(info.BinFileSender),
                byte.Parse(info.BinFileReceiver)
            };
            payLoadDataByteList.AddRange(Converter.GetReversedUShortBytes(0));
            payLoadDataByteList.AddRange(Converter.GetReversedUShortBytes(0));
            payLoadDataByteList.AddRange(Converter.GetReversedUShortBytes(info.PackageBinLength));
            payLoadDataByteList.AddRange(Converter.GetReversedUShortBytes(info.BinFileLength));
            payLoadDataByteList.AddRange(info.BinFileContent);

            PayloadData = payLoadDataByteList.ToArray();
            PayLoadLength = Converter.GetReversedUShortBytes((ushort)PayloadData.Length);
        }

        public byte[] Pop()
        {
            var bytes = CombinePackage();
            _currentIndex++;
            return CombinePackage();
        }
    }
}
