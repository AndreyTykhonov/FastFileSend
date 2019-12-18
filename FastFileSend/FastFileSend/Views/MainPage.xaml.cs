using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using FastFileSend.Models;
using Xamarin.Essentials;

namespace FastFileSend.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : MasterDetailPage
    {
        Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();
        public MainPage()
        {
            InitializeComponent();

            MasterBehavior = MasterBehavior.Popover;

            MenuPages.Add((int)MenuItemType.Downloads, (NavigationPage)Detail);

        }

        public async Task NavigateFromMenu(int id)
        {
            switch (id)
            {
                case (int)MenuItemType.Downloads:
                    MenuPages.Add(id, new NavigationPage(new ItemsPage()));
                    break;
                case (int)MenuItemType.Send:
                    await Global.FastFileSendProgramXamarin.Send();
                    break;
            }

            await Task.Delay(100);

            IsPresented = false;
        }
    }
}