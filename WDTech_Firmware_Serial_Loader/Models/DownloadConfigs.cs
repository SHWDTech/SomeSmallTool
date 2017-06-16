using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WDTech_Firmware_Serial_Loader.Data;

namespace WDTech_Firmware_Serial_Loader.Models
{
    public class DownloadConfigs
    {
        private static List<LocalConfig> _localConfigs;

        public static void InitConfigs(List<LocalConfig> configs)
        {
            _localConfigs = configs;
            var type = typeof(DownloadConfigs);
            var staticPropertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var propertyInfo in staticPropertyInfos)
            {
                var configItem = configs.FirstOrDefault(c => c.ConfigName == propertyInfo.Name);
                if (configItem == null) continue;
                var value = GetPropertyValue(configItem.ConfigValue, propertyInfo);
                propertyInfo.SetValue(null, value);
            }
        }

        public static ushort PackageBinFileLength { get; set; } = 256;

        public static ushort TimeOut { get; set; } = 60;

        public static object GetPropertyValue(string configValue, PropertyInfo info)
        {
            switch (info.PropertyType.ToString())
            {
                case "System.UInt16":
                    return ushort.Parse(configValue);
                default:
                    return configValue;
            }
        }

        public static void StoreConfigs()
        {
            var ctx = new FirmwareSerialLoaderSqliteContext();
            var type = typeof(DownloadConfigs);
            var staticPropertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var propertyInfo in staticPropertyInfos)
            {
                var configItem = _localConfigs.FirstOrDefault(c => c.ConfigName == propertyInfo.Name);
                if (configItem == null) continue;
                configItem.ConfigValue = propertyInfo.GetValue(null).ToString();
            }

            ctx.AddOrUpdate(_localConfigs);
        }
    }
}
