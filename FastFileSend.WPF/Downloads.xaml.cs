using FastFileSend.Main;
using FastFileSend.Main.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace FastFileSend.WPF
{
    /// <summary>
    /// Interaction logic for Downloads.xaml
    /// </summary>
    public partial class Downloads : UserControl
    {
        public Downloads()
        {
            InitializeComponent();
            IsEnabled = false;
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            HistoryViewModel historyModel = ListViewHistory.SelectedItem as HistoryViewModel;
            if (historyModel == null)
            {
                return;
            }

            FastFileSendApp ffsWindows = App.GetFFSInstance();

            await ffsWindows.Send(historyModel.File);
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: Some progress here

            while (App.GetFFSInstance() is null)
            {
                await Task.Delay(100);
            }

            FastFileSendApp ffsWindows = App.GetFFSInstance();

            ListViewHistory.DataContext = ffsWindows.HistoryListViewModel;
            ListViewHistory.ItemsSource = ffsWindows.HistoryListViewModel.List;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewHistory.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));
            //view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            IsEnabled = true;
        }
    }
}
