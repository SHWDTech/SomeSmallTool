using System.Collections.Generic;
using System.Linq;

namespace FirmwareDownloaderHelper
{
    public class BinFileOptionsHelper
    {
        public static void UpdateConfigDicts(List<ConfigDict> configs) => _binFileConfigDicts = configs; 

        private static List<ConfigDict> _binFileConfigDicts;

        public static string GetUpdateTarget(string optionKey)
        {
            return _binFileConfigDicts.Where(opt => opt.ItemType == "BinFileUpdateTarget")
                .FirstOrDefault(o => o.ItemKey == optionKey)?.ItemValue;
        }

        public static string GetUpdateMode(string optionKey)
        {
            return _binFileConfigDicts.Where(opt => opt.ItemType == "BinFileUpdateMode")
                .FirstOrDefault(o => o.ItemKey == optionKey)?.ItemValue;
        }
    }
}
