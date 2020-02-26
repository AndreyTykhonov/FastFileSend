using FastFileSend.Program;
using FastFileSend.UI;
using FastFileSend.Views;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FastFileSend
{
    class FastFileSendProgramXamarin : FastFileSendProgram
    {
        public override async Task<FileInfo> SelectFileAsync()
        {
            FileData fileData = await CrossFilePicker.Current.PickFile();

            if (fileData == null)
                return null;

            //string fileName = fileData.FileName;
            //string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);

            FileInfo fileInfo = new FileInfo
            { 
                Name = System.IO.Path.GetFileName(fileData.FileName),
                Content = fileData.GetStream(),
            };


            return fileInfo;
        }

        public override async Task<UserModel> SelectUserAsync()
        {
            TaskCompletionSource<UserModel> taskCompletionSource = new TaskCompletionSource<UserModel>();
            UserSelectPage userSelectPage = new UserSelectPage(taskCompletionSource);
            await Application.Current.MainPage.Navigation.PushModalAsync(new NavigationPage(userSelectPage) { Title = "Choose user" });

            return await taskCompletionSource.Task;
        }
    }
}
