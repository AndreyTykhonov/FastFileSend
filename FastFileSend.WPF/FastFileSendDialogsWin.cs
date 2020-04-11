using FastFileSend.Main.Interfaces;
using FastFileSend.Main.Models;
using FastFileSend.Main.ViewModel;
using FastFileSend.WPF.Pages;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
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
            using (var dialog = new CommonOpenFileDialog())
            {
                CommonFileDialogResult result = dialog.ShowDialog();

                if (result != CommonFileDialogResult.Ok)
                {
                    return null;
                }

                FileStream fs = new FileStream(dialog.FileName, FileMode.Open);
                Main.Models.FileInfo fileInfo = new Main.Models.FileInfo
                {
                    Name = Path.GetFileName(dialog.FileName),
                    Content = fs,
                };

                return await Task.FromResult(fileInfo).ConfigureAwait(false);
            }
        }


        public async Task<string> SelectFolderAsync()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                CommonFileDialogResult result = dialog.ShowDialog();

                if (result != CommonFileDialogResult.Ok)
                {
                    return null;
                }

                return await Task.FromResult(dialog.FileName).ConfigureAwait(false);
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
