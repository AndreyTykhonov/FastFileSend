using FastFileSend.Main;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FastFileSend
{
    class Updater
    {
        GithubVersionInfo GithubVersionInfo { get; set; }

        public async Task<bool> Available()
        {
            await FetchInfo().ConfigureAwait(false);

            return GithubVersionInfo.Tag != CurrentVersion();
        }

        private async Task FetchInfo()
        {
            if (GithubVersionInfo != null)
            {
                return;
            }

            GithubVersion githubVersion = new GithubVersion();
            GithubVersionInfo = await githubVersion.GetLastVersion("com.fastfilesend-Signed.apk").ConfigureAwait(false);
        }

        public async Task Update()
        {
            await FetchInfo().ConfigureAwait(false);
            string tempPath = Path.GetTempFileName() + ".apk";

            using (HttpClient http = new HttpClient())
            {
                using (FileStream fs = new FileStream(tempPath, FileMode.Create))
                {
                    Stream stream = await http.GetStreamAsync(GithubVersionInfo.Link);

                    
                    await stream.CopyToAsync(fs);
                }
            }

            IApkInstaller apkInstaller = DependencyService.Get<IApkInstaller>();
            apkInstaller.Launch(tempPath);
        }

        string CurrentVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion;
        }
    }
}
