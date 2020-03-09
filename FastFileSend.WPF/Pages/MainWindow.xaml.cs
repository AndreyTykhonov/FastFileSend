using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FastFileSend.Main;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.ComponentModel;
using MahApps.Metro.Controls.Dialogs;

namespace FastFileSend.WPF.Pages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void HamburgerMenu_ItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        {
            HamburgerMenuIconItem hamburgerMenuItem = e.InvokedItem as HamburgerMenuIconItem;

            if (hamburgerMenuItem == null)
            {
                return;
            }

            if (hamburgerMenuItem == HamburgerItemId)
            {
                Clipboard.SetText(HamburgerItemId.Label);
                HamburgerMenu.SelectedOptionsItem = null;

                await this.ShowMessageAsync("Fast File Send", "Your ID was copied to clipboard!");
            }

            switch (hamburgerMenuItem.Label)
            {
                case "History": 
                    HamburgerMenu.Content = e.InvokedItem;
                    break;
                case "Send file":
                    await App.FastFileSendApp.Send().ConfigureAwait(false);
                    break;
                case "Send folder":
                    await App.FastFileSendApp.SendFolder().ConfigureAwait(false);
                    break;
                case "Downloads":                    
                    Process.Start(new FastFileSendPathResolverWin().Downloads);
                    break;
            }

            HamburgerMenu.SelectedOptionsItem = null;
            HamburgerMenu.SelectedIndex = 0;
        }

        string GetAppVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: Some progress here
            
            while (App.FastFileSendApp is null)
            {
                await Task.Delay(100);
            }

            FastFileSendApp ffsWindows = App.FastFileSendApp;
            HamburgerItemId.Label = ffsWindows.AccountDetails.Id.ToString();

            Title = $"Fast File Send {GetAppVersion()}";
        }

        private async void MetroWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (Directory.Exists(files.First()))
                {
                    await App.FastFileSendApp.SendFolder(files.First()).ConfigureAwait(false);
                }
                else
                {
                    await App.FastFileSendApp.Send(files.First()).ConfigureAwait(false);
                }
            }
        }
    }
}
