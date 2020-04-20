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

        public async Task<GithubVersionInfo> VersionInfo()
        {
            return GithubVersionInfo;
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

        string CurrentVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string version = assembly.GetName().Version.ToString();
            return version;
        }
    }
}
