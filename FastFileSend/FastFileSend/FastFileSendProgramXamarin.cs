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
        public override async Task<string> SelectFileAsync()
        {
            FileData fileData = await CrossFilePicker.Current.PickFile();

            if (fileData == null)
                return string.Empty;

            //string fileName = fileData.FileName;
            //string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);

            return fileData.FilePath;
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
