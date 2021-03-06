﻿using System.Windows;
using WDTech_Frimware_Tcp_Loader.Models;

namespace WDTech_Frimware_Tcp_Loader.Views
{
    /// <summary>
    /// Interaction logic for DownloadSetting.xaml
    /// </summary>
    public partial class DownloadSetting
    {
        public DownloadSetting()
        {
            InitializeComponent();
        }

        private void ApplyLocalConfigs(object sender, RoutedEventArgs e)
        {
            DownloadConfigs.PackageBinFileLength = ushort.Parse(TxtPackageBinFIleLength.Text);
            DownloadConfigs.TimeOut = ushort.Parse(TxtTimeOut.Text);
            DownloadConfigs.StoreConfigs();
        }

        private void ApplyAndLeave(object sender, RoutedEventArgs e)
        {
            ApplyLocalConfigs(sender, e);
            Close();
        }
    }
}
