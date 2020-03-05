using FastFileSend.Main;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace FastFileSend.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static FastFileSendApp FastFileSendApp { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            FastFileSendApp = await FastFileSendApp.Create(new FastFileSendPathResolverWin(), new FastFileSendDialogsWin());
        }
    }
}
