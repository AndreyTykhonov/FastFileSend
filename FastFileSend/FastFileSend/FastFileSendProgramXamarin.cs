using FastFileSend.Program;
using FastFileSend.UI;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

        public override UserModel SelectUser()
        {
            return base.SelectUser();
        }
    }
}
