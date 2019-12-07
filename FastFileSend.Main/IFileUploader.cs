using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.Main
{
    public interface IFileUploader
    {
        Task<CloudFile> UploadAsync(string path);
    }
}
