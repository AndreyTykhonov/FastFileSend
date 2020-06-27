using FastFileSend.Main.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FastFileSend.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class DownloadsPage : Xamarin.Forms.TabbedPage
    {
        public DownloadsPage()
        {
            InitializeComponent();
        }

        private async void MenuItem_Clicked(object sender, EventArgs e)
        {
            HistoryViewModel selected = (sender as MenuItem).CommandParameter as HistoryViewModel;

            if (selected == null)
            {
                return;
            }

            await App.FastFileSendApp.Send(selected.File);
        }

        private async void ContentPage_Appearing(object sender, EventArgs e)
        {
            base.OnAppearing();

            // TODO: Some loading progress

            while (App.FastFileSendApp is null)
            {
                await Task.Delay(100);
            }

            ItemsListView.BindingContext = App.FastFileSendApp.HistoryListViewModel;
        }
    }
}