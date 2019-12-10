using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FastFileSend.Main
{
    public class DummyFileUploader : IFileUploader
    {
        public event Action<double, double> OnProgress = delegate { };
        public event Action OnEnd = delegate { };

        public async Task<CloudFile> UploadAsync(string path)
        {
            await Task.Delay(1000);
            OnProgress(25, 1);
            await Task.Delay(1000);
            OnProgress(50, 2);
            await Task.Delay(1000);
            OnProgress(100, 3);
            OnEnd();

            return new CloudFile(0, "heh", 0, DateTime.Now, "da");
        }
    }
}
