using FastFileSend.Main;
using FastFileSend.UI;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FastFileSend.Program
{
    public class FastFileSendProgram
    {
        public HistoryViewModel HistoryViewModel { get; set; }
        public UserViewModel UserViewModel { get; set; }
        public ApiServer ApiServer { get; set; }

        public async Task Login(int id, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                ApiServer = await ApiServer.CreateNewAccount();
            }
            else
            {
                ApiServer = new ApiServer(id, password);
            }


            HistoryViewModel = new HistoryViewModel(ApiServer);

            UserViewModel = new UserViewModel(ApiServer, HistoryViewModel);

            HistoryViewModel.List.CollectionChanged += List_CollectionChanged;
        }

        private async void List_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
            {
                return;
            }

            HistoryModel model = (HistoryModel)e.NewItems[0];

            if (model.Fake)
            {
                return;
            }

            if (model.Status == HistoryModelStatus.Ok)
            {
                return;
            }

            if (model.Receiver != ApiServer.Id)
            {
                return;
            }

            FileDownloader fileDownloader = new FileDownloader();

            FileItem fileItem = new FileItem()
            {
                Name = model.Name,
                Url = model.Url
            };

            model.Status = HistoryModelStatus.Downloading;

            fileDownloader.OnProgress += (double progress, double speed) =>
            {
                model.Progress = progress;
                model.ETA = speed.ToString("0.00 MB/s");
            };

            await fileDownloader.DownloadAsync(fileItem);

            model.Status = HistoryModelStatus.Ok;
            model.ETA = "";

            await ApiServer.NotifyDownloadedAsync(model.Id);
        }

        public virtual async Task<UserModel> SelectUserAsync()
        {
            return new UserModel();
        }

        public virtual async Task<string> SelectFileAsync()
        {
            return string.Empty;
        }

        public async Task Send()
        {
            UserModel target = await SelectUserAsync();

            if (target == null)
            {
                return;
            }

            string filePath = await SelectFileAsync();

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            await SendFile(target, filePath);
        }

        private async Task SendFile(UserModel target, string path)
        {
            string filename = Path.GetFileName(path);
            if (filename.Length >= 300)
            {
                return;
            }

            FileStream fs = new FileStream(path, FileMode.Open);
            HistoryModel downloadModel = new HistoryModel
            {
                Name = System.IO.Path.GetFileName(path),
                Status = HistoryModelStatus.Uploading,
                ETA = "",
                Receiver = target.Id,
                Sender = ApiServer.Id,
                Fake = true,
                Size = fs.Length,
                Date = DateTime.Now
            };
            fs.Close();

            HistoryViewModel.List.Insert(0, downloadModel);

            //IFileUploader fileUploader = new DummyFileUploader();
            IFileUploader fileUploader = new FexFileUploader();
            fileUploader.OnProgress += (double progress, double speed) =>
            {
                downloadModel.Progress = progress;
                downloadModel.ETA = speed.ToString("0.00 MB/s");
            };

            CloudFile cloudFile = await fileUploader.UploadAsync(path);

            downloadModel.Progress = 100;
            downloadModel.Status = HistoryModelStatus.UsingAPI;

            //cloudFile = new CloudFile(0, "debug.zip", 0, DateTime.Now, "https://cdn.shazoo.ru/393609_KPsmQaHsNk_382993_uuwtofdnti_fb6f81c359f7cd.jpg");

            FileItem uploadedFile = await ApiServer.Upload(cloudFile);

            try
            {
                int download_index = await ApiServer.Send(uploadedFile, target.Id);
                downloadModel.Id = download_index;

                downloadModel.Status = HistoryModelStatus.Awaiting;
            }
            catch (HttpRequestException)
            {
                HistoryViewModel.List.Remove(downloadModel);
            }
        }
    }
}
