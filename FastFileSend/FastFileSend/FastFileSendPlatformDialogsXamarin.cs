using FastFileSend.Main.Interfaces;
using FastFileSend.Main.Models;
using FastFileSend.Main.ViewModel;
using FastFileSend.Views;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace FastFileSend
{
    class FastFileSendPlatformDialogsXamarin : IFastFileSendPlatformDialogs
    {
        public async Task<FileInfo> SelectFileAsync()
        {
            FileResult fileData = await FilePicker.PickAsync();

            if (fileData == null)
                return null;

            FileInfo fileInfo = new FileInfo
            {
                Name = System.IO.Path.GetFileName(fileData.FileName),
                Content = await fileData.OpenReadAsync(),
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
