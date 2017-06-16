using System;
using System.Configuration;
using System.IO;
using System.Windows;
using FirmwareDownloaderHelper;
using WDTech_Firmware_Serial_Loader.Data;
using WDTech_Firmware_Serial_Loader.Models;

namespace WDTech_Firmware_Serial_Loader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Init();
            LoadInitLocalData();
        }

        private void Init()
        {
            FirmwareSerialLoaderSqliteContext.DefaultConnectinoString = string.Format(ConfigurationManager.AppSettings["dbConnStr"], Directory.GetCurrentDirectory());
        }

        private void LoadInitLocalData()
        {
            try
            {
                var ctx = new FirmwareSerialLoaderSqliteContext();
                BinFileOptionsHelper.UpdateConfigDicts(ctx.ConfigDicts);
                DownloadConfigs.InitConfigs(ctx.LocalConfigs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
