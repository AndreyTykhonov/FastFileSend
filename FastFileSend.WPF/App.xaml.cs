using FastFileSend.Main;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FastFileSend.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public FastFileSendProgramWindows FastFileSendProgramWindows { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            FastFileSendProgramWindows = new FastFileSendProgramWindows();

            if (!File.Exists(FilePathHelper.AccountConfig))
            {
                FastFileSendProgramWindows.CreateAccountDetails(FastFileSend.WPF.Properties.Settings.Default.id, FastFileSend.WPF.Properties.Settings.Default.password);
            }

            FastFileSendProgramWindows.Login().Wait();
        }
    }
}
