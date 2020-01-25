using FastFileSend.Main;
using FastFileSend.UI;
using Newtonsoft.Json;
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

        public async Task Login(HttpClientHandler httpClientHandler)
        {
            if (File.Exists(FilePathHelper.AccountConfig))
            {
                AccountDetails accountDetails = JsonConvert.DeserializeObject<AccountDetails>(File.ReadAllText(FilePathHelper.AccountConfig));
                ApiServer = new ApiServer(accountDetails.Id, accountDetails.Password, httpClientHandler);
            }
            else
            {
                ApiServer = await ApiServer.CreateNewAccount(httpClientHandler);
                AccountDetails accountDetails = new AccountDetails { Id = ApiServer.Id, Password = ApiServer.Password };
                string json = JsonConvert.SerializeObject(accountDetails);
                File.WriteAllText(FilePathHelper.AccountConfig, json);
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

            FileItem fileItem = new FileItem(0, model.Name, model.Size, 0, model.Date, model.Url);

            model.Status = HistoryModelStatus.Downloading;

            fileDownloader.OnProgress += (double progress, double speed) =>
            {
                model.Progress = progress;
                model.ETA = SizeUtils.BytesToString(Convert.ToInt32(speed), "/s");
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

            string filename = Path.GetFileName(filePath);
            if (filename.Length >= 300)
            {
                return;
            }

            HistoryModel historyModel = HistoryModelAdd(filePath, target);

            try
            {
                FileItem uploadedFile = await UploadFile(filePath, historyModel);
                await SendFile(target, uploadedFile, historyModel);
            }
            catch (IOException)
            {
                HistoryViewModel.List.Remove(historyModel);
            }
        }

        public async Task Send(FileItem fileItem)
        {
            UserModel target = await SelectUserAsync();

            if (target == null)
            {
                return;
            }

            HistoryModel historyModel = HistoryModelAdd(fileItem, target);
            await SendFile(target, fileItem, historyModel);
        }

        HistoryModel HistoryModelAdd(string path, UserModel target)
        {
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

            return downloadModel;
        }

        HistoryModel HistoryModelAdd(FileItem uploadedFile, UserModel target)
        {
            HistoryModel downloadModel = new HistoryModel
            {
                Name = uploadedFile.Name,
                Status = HistoryModelStatus.Uploading,
                ETA = "",
                Receiver = target.Id,
                Sender = ApiServer.Id,
                Fake = true,
                Size = uploadedFile.Size,
                Date = DateTime.Now
            };

            HistoryViewModel.List.Insert(0, downloadModel);

            return downloadModel;
        }

        async Task<FileItem> UploadFile(string path, HistoryModel downloadModel)
        {
            //IFileUploader fileUploader = new DummyFileUploader();
            FexFileUploader fileUploader = new FexFileUploader();
            fileUploader.OnProgress += (double progress, double speed) =>
            {
                downloadModel.Progress = progress;
                downloadModel.ETA = SizeUtils.BytesToString(Convert.ToInt32(speed), "/s");
            };

            FileItem fileItem = await fileUploader.UploadAsync(path);

            downloadModel.Progress = 100;
            downloadModel.Status = HistoryModelStatus.UsingAPI;

            return await ApiServer.Upload(fileItem);
        }

        private async Task SendFile(UserModel target, FileItem uploadedFile, HistoryModel downloadModel)
        {
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
