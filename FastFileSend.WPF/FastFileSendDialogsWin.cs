using FastFileSend.Main.Interfaces;
using FastFileSend.Main.Models;
using FastFileSend.Main.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.WPF
{
    class FastFileSendDialogsWin : IFastFileSendPlatformDialogs
    {
        public async Task<Main.Models.FileInfo> SelectFileAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (!(bool)openFileDialog.ShowDialog())
            {
                return null;
            }

            FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open);
            Main.Models.FileInfo fileInfo = new Main.Models.FileInfo
            {
                Name = Path.GetFileName(openFileDialog.FileName),
                Content = fs,
            };

            return fileInfo;
        }

        public async Task<UserViewModel> SelectUserAsync(UserListViewModel userListViewModel)
        {
            UsersWindow usersWindow = new UsersWindow(userListViewModel);
            usersWindow.ShowDialog();

            return userListViewModel.Selected;
        }
    }
}
