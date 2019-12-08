using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using FastFileSend.Xamarin.Services;
using FastFileSend.Xamarin.Views;

namespace FastFileSend.Xamarin
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
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
