using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FastFileSend.Views;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System.IO;
using FastFileSend.Main;
using System.Net.Http;

namespace FastFileSend
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override async void OnStart()
        {
            HttpClientHandler httpClientHandler = DependencyService.Get<IHttpClientService>().Handler;

            await Global.FastFileSendProgramXamarin.Login(httpClientHandler);

            Preferences.Set("id", Global.FastFileSendProgramXamarin.ApiServer.Id);
            Preferences.Set("password", Global.FastFileSendProgramXamarin.ApiServer.Password);

            MasterDetailPage masterDetailPage = MainPage as MasterDetailPage;
            MenuPage menu = masterDetailPage.Master as MenuPage;

            menu.EntryId.Text = Global.FastFileSendProgramXamarin.ApiServer.Id.ToString();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
