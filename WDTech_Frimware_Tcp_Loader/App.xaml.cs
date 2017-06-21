using System;
using System.Configuration;
using System.IO;
using System.Windows;
using FirmwareDownloaderHelper;
using WDTech_Frimware_Tcp_Loader.Data;
using WDTech_Frimware_Tcp_Loader.Models;

namespace WDTech_Frimware_Tcp_Loader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!Init())
            {
                MessageBox.Show("程序基础配置信息丢失，请联系开发人员！");
                Shutdown();
                return;
            }
            base.OnStartup(e);
        }

        private bool Init()
        {
            try
            {
                var dbLocation = string.Format(ConfigurationManager.AppSettings["dbLocation"], Directory.GetCurrentDirectory());
                var dbConn = string.Format(ConfigurationManager.AppSettings["dbConnStr"], dbLocation);
                FirmwareSerialLoaderSqliteContext.DefaultConnectinoString = dbConn;
                var ctx = new FirmwareSerialLoaderSqliteContext();
                BinFileOptionsHelper.UpdateConfigDicts(ctx.ConfigDicts);
                DownloadConfigs.InitConfigs(ctx.LocalConfigs);
                return true;
            }
            catch (Exception ex)
            {
                SimpleLog.Fatal("加载数据库信息失败！", ex);
                return false;
            }
        }
    }
}
