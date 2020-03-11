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

            //string fileName = fileData.FileName;
            //string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);

            if (!fileData.GetStream().CanRead)
            {
                return null;
            }

            FileInfo fileInfo = new FileInfo
            {
                Name = System.IO.Path.GetFileName(fileData.FileName),
                Content = fileData.GetStream(),
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
