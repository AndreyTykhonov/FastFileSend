using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FastFileSend.Main
{
    public static class FilePathHelper
    {
        static string AppDataFolder { get; set; }

        static FilePathHelper()
        {
            AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            if (AppDataFolder.Contains(":"))
            {
                AppDataFolder = Path.Combine(AppDataFolder, "FFS");

                if (!Directory.Exists(AppDataFolder))
                {
                    Directory.CreateDirectory(AppDataFolder);
                }
            }
        }

        public static string UsersConfig
        {
            get
            {
                return Path.Combine(AppDataFolder, "users.json");
            }
        }
    }
}
