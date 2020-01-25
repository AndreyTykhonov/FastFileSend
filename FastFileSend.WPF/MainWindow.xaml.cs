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
using FastFileSend.UI;
using FastFileSend.Main;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.ComponentModel;
using MahApps.Metro.Controls.Dialogs;

namespace FastFileSend.WPF
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
                HamburgerMenu.SelectedIndex = 0;

                await this.ShowMessageAsync("Fast File Send", "Your ID was copied to clipboard!");
            }

            switch (hamburgerMenuItem.Label)
            {
                case "History": 
                    HamburgerMenu.Content = e.InvokedItem;
                    break;
                case "Send file":
                    FastFileSendProgramWindows ffsWindows = (Application.Current as App).FastFileSendProgramWindows;
                    await ffsWindows.Send();
                    HamburgerMenu.SelectedIndex = 0;
                    break;
                case "Downloads":                    
                    Process.Start(FilePathHelper.Downloads);
                    HamburgerMenu.SelectedIndex = 0;
                    break;
                default:
                    HamburgerMenu.SelectedIndex = 0;
                    break;
            }
        }

        string GetAppVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FastFileSendProgramWindows ffsWindows = (Application.Current as App).FastFileSendProgramWindows;
            HamburgerItemId.Label = ffsWindows.ApiServer.Id.ToString();

            Title = $"Fast File Send {GetAppVersion()}";
        }
    }
}
