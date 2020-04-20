using FastFileSend.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FastFileSend.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdatePage : ContentPage
    {
        GithubVersionInfo UpdateVersionInfo {get; set;}

        public UpdatePage(GithubVersionInfo versionInfo)
        {
            InitializeComponent();

            UpdateVersionInfo = versionInfo;
            LabelUpdate.Text = $"Downloading update {UpdateVersionInfo.Tag}";
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await Update().ConfigureAwait(false);
            Environment.Exit(0);
        }

        public async Task Update()
        {
            string tempPath = Path.GetTempFileName() + ".apk";

            using (HttpClient http = new HttpClient())
            {
                using (FileStream fs = new FileStream(tempPath, FileMode.Create))
                {
                    Stream stream = await http.GetStreamAsync(UpdateVersionInfo.Link).ConfigureAwait(false);

                    await stream.CopyToAsync(fs).ConfigureAwait(false);
                }
            }

            IApkInstaller apkInstaller = DependencyService.Get<IApkInstaller>();
            apkInstaller.Launch(tempPath);
        }
    }
}