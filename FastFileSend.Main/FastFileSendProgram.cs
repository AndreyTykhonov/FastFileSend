using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FastFileSend.Main
{
    public class FastFileSendProgram
    {
        public HistoryViewModel HistoryViewModel { get; set; }
        public UserViewModel UserViewModel { get; set; }
        public ApiServer ApiServer { get; set; }

        public bool Ready { get; set; } = false;
        public FileInfo FileInfo { get; private set; }

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

            await ApiServer.Login();

            HistoryViewModel = new HistoryViewModel(ApiServer);

            UserViewModel = new UserViewModel(ApiServer, HistoryViewModel);

            HistoryViewModel.List.CollectionChanged += List_CollectionChanged;

            Ready = true;
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

        public virtual async Task<FileInfo> SelectFileAsync()
        {
            return new FileInfo();
        }

        public async Task Send()
        {
            UserModel target = await SelectUserAsync();

            if (target == null)
            {
                return;
            }

            FileInfo fileInfo  = await SelectFileAsync();

            if (fileInfo == null)
            {
                return;
            }

            if (fileInfo.Name.Length >= 300)
            {
                return;
            }

            HistoryModel historyModel = null;

            try
            {
                historyModel = HistoryModelAdd(fileInfo.Name, fileInfo.Content.Length, target);
                FileItem uploadedFile = await UploadFile(fileInfo, historyModel);
                await SendFile(target, uploadedFile, historyModel);
            }
            catch (IOException)
            {
                if (historyModel != null)
                {
                    HistoryViewModel.List.Remove(historyModel);
                }
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

        HistoryModel HistoryModelAdd(string filename, long size, UserModel target)
        {
            HistoryModel downloadModel = new HistoryModel
            {
                Name = filename,
                Status = HistoryModelStatus.Uploading,
                ETA = "",
                Receiver = target.Id,
                Sender = ApiServer.Id,
                Fake = true,
                Size = size,
                Date = DateTime.Now
            };

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

        async Task<FileItem> UploadFile(FileInfo fileInfo, HistoryModel downloadModel)
        {
            //IFileUploader fileUploader = new DummyFileUploader();
            FexFileUploader fileUploader = new FexFileUploader();
            fileUploader.OnProgress += (double progress, double speed) =>
            {
                downloadModel.Progress = progress;
                downloadModel.ETA = SizeUtils.BytesToString(Convert.ToInt32(speed), "/s");
            };

            FileItem fileItem = await fileUploader.UploadAsync(fileInfo.Name, fileInfo.Content);

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
