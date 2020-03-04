using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using FastFileSend.Models;
using FastFileSend.Views;
using FastFileSend.UI;

namespace FastFileSend.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ItemsPage : ContentPage
    {
        public ItemsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ItemsListView.BindingContext = Global.FastFileSendProgramXamarin.HistoryViewModel;
        }

        private async void MenuItem_Clicked(object sender, EventArgs e)
        {
            HistoryModel selected = (sender as MenuItem).CommandParameter as HistoryModel;

            if (selected == null)
            {
                return;
            }

            await Global.FastFileSendProgramXamarin.Send(selected.File);
        }
    }
}