using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FastFileSend.Main.Utils
{
    public static class PathUtils
    {
        static string GetFFSSettingsFolder()
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                appdata = Path.Combine(appdata, "FFS");

                if (!Directory.Exists(appdata))
                {
                    Directory.CreateDirectory(appdata);
                }
            }

            return appdata;
        }

        public static string UsersConfig
        {
            get
            {
                return Path.Combine(GetFFSSettingsFolder(), "users.json");
            }
        }

        public static string AccountConfig
        {
            get
            {
                return Path.Combine(GetFFSSettingsFolder(), "account.json");
            }
        }

        public static string Downloads
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    return "/sdcard/Download";
                }

                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }
        }
    }
}
