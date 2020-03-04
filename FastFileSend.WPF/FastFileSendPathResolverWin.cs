using FastFileSend.Main.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.WPF
{
    class FastFileSendPathResolverWin : IPathResolver
    {
        static string GetFFSSettingsFolder()
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            appdata = Path.Combine(appdata, "FFS");

            if (!Directory.Exists(appdata))
            {
                Directory.CreateDirectory(appdata);
            }

            return appdata;
        }

        public string UsersConfig => Path.Combine(GetFFSSettingsFolder(), "users.json");

        public string AccountConfig => Path.Combine(GetFFSSettingsFolder(), "account.json");

        public string Downloads => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    }
}
