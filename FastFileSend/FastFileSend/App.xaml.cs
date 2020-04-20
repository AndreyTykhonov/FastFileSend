using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FastFileSend.Views;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System.IO;
using FastFileSend.Main;
using System.Net.Http;
using FastFileSend.Main.Interfaces;

namespace FastFileSend
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        public static FastFileSendApp FastFileSendApp { get; private set; }

        protected override async void OnStart()
        {
            IPathResolver pathResolver = DependencyService.Get<IPathResolver>();

            Updater updater = new Updater();
            if (await updater.Available().ConfigureAwait(true))
            {
                UpdatePage updatePage = new UpdatePage(await updater.VersionInfo());
                await MainPage.Navigation.PushModalAsync(updatePage);
                //await updater.Update().ConfigureAwait(true);
                //Environment.Exit(0);
                return;
            }

            FastFileSendApp = await FastFileSendApp.Create(pathResolver, new FastFileSendPlatformDialogsXamarin());

            Preferences.Set("id", FastFileSendApp.AccountDetails.Id);
            Preferences.Set("password", FastFileSendApp.AccountDetails.Password);

            MasterDetailPage masterDetailPage = MainPage as MasterDetailPage;
            MenuPage menu = masterDetailPage.Master as MenuPage;

            menu.EntryId.Text = FastFileSendApp.AccountDetails.Id.ToString();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
