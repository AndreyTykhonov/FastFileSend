using FastFileSend.Main.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.Main.Interfaces
{
    public interface IFastFileSendPlatformDialogs
    {
        Task<UserModel> SelectUserAsync();

        Task<FileInfo> SelectFileAsync();
    }
}
