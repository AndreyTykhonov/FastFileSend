using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace FastFileSend.Main
{
    public static class FilePathHelper
    {
        public static string UsersConfig
        {
            get
            {
                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    appdata = Path.Combine(appdata, "FFS");
                }

                return Path.Combine(appdata, "users.json");
            }
        }

        public static string Downloads
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    return "/storage/emulated/0/Download";
                }

                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }
        }
    }
}
