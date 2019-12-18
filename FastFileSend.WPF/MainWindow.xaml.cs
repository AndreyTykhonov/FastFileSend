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

            if (!File.Exists(FilePathHelper.AccountConfig))
            {
                FastFileSendProgramWindows.CreateAccountDetails(Properties.Settings.Default.id, Properties.Settings.Default.password);
            }

            await FastFileSendProgramWindows.Login();

            TextBlockId.Text = FastFileSendProgramWindows.ApiServer.Id.ToString();

            ListViewHistory.DataContext = FastFileSendProgramWindows.HistoryViewModel;
            ListViewHistory.ItemsSource = FastFileSendProgramWindows.HistoryViewModel.List;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewHistory.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));
            //view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            IsEnabled = true;
        }
    }
}
