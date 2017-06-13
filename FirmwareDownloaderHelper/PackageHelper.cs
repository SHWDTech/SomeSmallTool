namespace FirmwareDownloaderHelper
{
    public class PackageHelper
    {
        private BinInfo _binInfo;

        public void InitHelper(BinInfo info)
        {
            _binInfo = info;
        }

        public byte[] Pop()
        {
            var pkg = new FirmwareUpdatePackage(_binInfo);
            return pkg.Pop();
        }
    }
}
