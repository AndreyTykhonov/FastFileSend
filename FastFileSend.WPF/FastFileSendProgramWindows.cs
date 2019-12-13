using FastFileSend.Program;
using FastFileSend.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.WPF
{
    public class FastFileSendProgramWindows : FastFileSendProgram
    {
        public override async Task<string> SelectFileAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            return (bool)openFileDialog.ShowDialog() ? openFileDialog.FileName : string.Empty;
        }

        public override async Task<UserModel> SelectUserAsync()
        {
            UsersWindow usersWindow = new UsersWindow(UserViewModel);
            usersWindow.ShowDialog();

            return UserViewModel.Selected;
        }
    }
}
