using FastFileSend.Main;
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
            HistoryModel historyModel = ListViewHistory.SelectedItem as HistoryModel;
            if (historyModel == null)
            {
                return;
            }

            FastFileSendProgramWindows ffsWindows = App.GetInstance().FastFileSendProgramWindows;

            await ffsWindows.Send(historyModel.File);
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FastFileSendProgramWindows ffsWindows = App.GetInstance().FastFileSendProgramWindows;

            while (!ffsWindows.Ready)
            {
                await Task.Delay(100);
            }

            ListViewHistory.DataContext = ffsWindows.HistoryViewModel;
            ListViewHistory.ItemsSource = ffsWindows.HistoryViewModel.List;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewHistory.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));
            //view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            IsEnabled = true;
        }
    }
}
