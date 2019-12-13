using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FastFileSend.Services;
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

            DependencyService.Register<MockDataStore>();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            int id = Convert.ToInt32(Preferences.Get("id", 0));
            string password = Preferences.Get("password", string.Empty);

            id = 555045;
            password = "601941791";

            Global.FastFileSendProgramXamarin.Login(id, password).Wait();

            Preferences.Set("id", Global.FastFileSendProgramXamarin.ApiServer.Id);
            Preferences.Set("password", Global.FastFileSendProgramXamarin.ApiServer.Password);
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
