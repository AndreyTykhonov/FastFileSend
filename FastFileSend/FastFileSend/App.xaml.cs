using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FastFileSend.Views;
using System.Threading.Tasks;
using Xamarin.Essentials;

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
            /*
            if ((await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage)) != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
            {
                var permissions = await CrossPermissions.Current.RequestPermissionsAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                if (!permissions.TryGetValue(Plugin.Permissions.Abstractions.Permission.Storage, out Plugin.Permissions.Abstractions.PermissionStatus status))
                {
                    Quit();
                }

                if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                {
                    Quit();
                }
            }
            */

            int id = Convert.ToInt32(Preferences.Get("id", 0));
            string password = Preferences.Get("password", string.Empty);

            await Global.FastFileSendProgramXamarin.Login(id, password);

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
