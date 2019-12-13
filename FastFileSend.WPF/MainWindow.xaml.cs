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

namespace FastFileSend.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        FastFileSendProgramWindows FastFileSendProgramWindows { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            IsEnabled = false;
        }


        private async void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            await FastFileSendProgramWindows.Send();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonDownloads_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(FilePathHelper.Downloads);
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FastFileSendProgramWindows = new FastFileSendProgramWindows();
            await FastFileSendProgramWindows.Login(Properties.Settings.Default.id, Properties.Settings.Default.password);

            Properties.Settings.Default.id = FastFileSendProgramWindows.ApiServer.Id;
            Properties.Settings.Default.password = FastFileSendProgramWindows.ApiServer.Password;
            Properties.Settings.Default.Save();

            TextBlockId.Text = FastFileSendProgramWindows.ApiServer.Id.ToString();

            ListViewHistory.DataContext = FastFileSendProgramWindows.HistoryViewModel;

            IsEnabled = true;
        }
    }
}
