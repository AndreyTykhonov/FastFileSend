using FastFileSend.Main;
using FastFileSend.WPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FastFileSend.WPF
{
    public class FastFileSendProgramWindows : FastFileSendProgram
    {
        public override async Task<Main.FileInfo> SelectFileAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (!(bool)openFileDialog.ShowDialog())
            {
                return null;
            }

            FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open);
            Main.FileInfo fileInfo = new Main.FileInfo
            {
                Name = Path.GetFileName(openFileDialog.FileName),
                Content = fs,
            };

            return fileInfo;
        }

        public override async Task<UserModel> SelectUserAsync()
        {
            UsersWindow usersWindow = new UsersWindow(UserViewModel);
            usersWindow.ShowDialog();

            return UserViewModel.Selected;
        }
    }
}
