using FastFileSend.Program;
using FastFileSend.UI;
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
        public override async Task<Program.FileInfo> SelectFileAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (!(bool)openFileDialog.ShowDialog())
            {
                return null;
            }

            FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open);
            Program.FileInfo fileInfo = new Program.FileInfo
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
