using FastFileSend.Main.Models;
using FastFileSend.Main.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.Main.Interfaces
{
    public interface IFastFileSendPlatformDialogs
    {
        Task<UserViewModel> SelectUserAsync(UserListViewModel userListViewModel);

        Task<FileInfo> SelectFileAsync();
    }
}
