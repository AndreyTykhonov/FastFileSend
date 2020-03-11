using FastFileSend.Main.Interfaces;
using FastFileSend.Main.Models;
using FastFileSend.Main.ViewModel;
using FastFileSend.Views;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FastFileSend
{
    class FastFileSendPlatformDialogsXamarin : IFastFileSendPlatformDialogs
    {
        public async Task<FileInfo> SelectFileAsync()
        {
            FileData fileData = await CrossFilePicker.Current.PickFile();       

            if (fileData == null)
                return null;

            System.IO.MemoryStream stream = new System.IO.MemoryStream(fileData.DataArray);

            FileInfo fileInfo = new FileInfo
            {
                Name = System.IO.Path.GetFileName(fileData.FileName),
                Content = stream,
            };

            return fileInfo;
        }

        public Task<string> SelectFolderAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<UserModel> SelectUserAsync(UserListViewModel userListViewModel)
        {
            TaskCompletionSource<UserViewModel> taskCompletionSource = new TaskCompletionSource<UserViewModel>();
            UserSelectPage userSelectPage = new UserSelectPage(taskCompletionSource);
            await Application.Current.MainPage.Navigation.PushModalAsync(new NavigationPage(userSelectPage) { Title = "Choose user" });

            return await taskCompletionSource.Task;
        }
    }
}
