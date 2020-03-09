using FastFileSend.Main.Interfaces;
using FastFileSend.Main.Models;
using FastFileSend.Main.ViewModel;
using FastFileSend.WPF.Pages;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FastFileSend.WPF
{
    class FastFileSendDialogsWin : IFastFileSendPlatformDialogs
    {
        public async Task<Main.Models.FileInfo> SelectFileAsync()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
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

            return await Task.FromResult(fileInfo).ConfigureAwait(false);
        }

        public async Task<string> SelectFolderAsync()
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                {
                    return null;
                }

                return await Task.FromResult(folderBrowserDialog.SelectedPath).ConfigureAwait(false);
            }
        }

        public async Task<UserModel> SelectUserAsync(UserListViewModel userListViewModel)
        {
            UsersWindow usersWindow = new UsersWindow(userListViewModel);
            usersWindow.ShowDialog();

            return await Task.FromResult(userListViewModel.Selected).ConfigureAwait(false);
        }
    }
}
