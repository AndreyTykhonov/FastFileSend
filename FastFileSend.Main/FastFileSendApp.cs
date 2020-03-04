using FastFileSend.Main.Enum;
using FastFileSend.Main.Interfaces;
using FastFileSend.Main.Models;
using FastFileSend.Main.RemoteFile;
using FastFileSend.Main.Utils;
using FastFileSend.Main.ViewModel;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FastFileSend.Main
{
    /// <summary>
    /// Send and retreive files in background.
    /// </summary>
    public class FastFileSendApp
    {
        public HistoryListViewModel HistoryListViewModel { get; private set; }
        public UserListViewModel UseristViewModel { get; private set; }

        public event Action OnLoaded = delegate {};
        public event Action<HistoryViewModel> OnDownloaded = delegate { }; 

        private Api ApiServer { get; set; }

        private HistoryViewModelUpdater HistoryViewModelUpdater { get; set; }
        private UserListViewModelUpdater UserListViewModelUpdateStatus { get; set; }

        private IPathResolver PathResolver { get; set; }
        private IFastFileSendPlatformDialogs FileSendPlatformDialogs { get; set; }

        private FastFileSendApp(IPathResolver pathResolver, IFastFileSendPlatformDialogs fileSendPlatformDialogs)
        {
            PathResolver = pathResolver;
            FileSendPlatformDialogs = fileSendPlatformDialogs;
        }

        /// <summary>
        /// This class should have async constructor, so you need to use this method.
        /// </summary>
        /// <param name="pathResolver">Platform path resolver.</param>
        /// <param name="fileSendPlatformDialogs">Platform dialogs resolver.</param>
        /// <returns></returns>
        public async Task<FastFileSendApp> Create(IPathResolver pathResolver, IFastFileSendPlatformDialogs fileSendPlatformDialogs)
        {
            if (File.Exists(pathResolver.AccountConfig))
            {
                AccountDetails accountDetails = JsonConvert.DeserializeObject<AccountDetails>(File.ReadAllText(pathResolver.AccountConfig));
                ApiServer = await Api.Login(accountDetails);
            }
            else
            {
                ApiServer = await Api.CreateNewAccount();
                string json = JsonConvert.SerializeObject(ApiServer.AccountDetails);
                File.WriteAllText(pathResolver.AccountConfig, json);
            }

            HistoryListViewModel = new HistoryListViewModel();
            UseristViewModel = new UserListViewModel(ApiServer, PathResolver.UsersConfig);

            HistoryListViewModel.List.CollectionChanged += HistoryListView_CollectionChanged;

            HistoryViewModelUpdater = new HistoryViewModelUpdater(ApiServer, HistoryListViewModel);
            UserListViewModelUpdateStatus = new UserListViewModelUpdater(ApiServer, UseristViewModel);

            OnLoaded();

            return new FastFileSendApp(pathResolver, fileSendPlatformDialogs);
        }

        /// <summary>
        /// Check if file needs to be downloaded when HistoryListView updates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void HistoryListView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
            {
                return;
            }

            HistoryViewModel model = (HistoryViewModel)e.NewItems[0];

            if (model.Fake)
            {
                return;
            }

            if (model.Status == HistoryModelStatus.Ok)
            {
                return;
            }

            if (model.Receiver != ApiServer.AccountDetails.Id)
            {
                return;
            }

            await Download(model);
        }

        private async Task Download(HistoryViewModel model)
        {
            FileDownloader fileDownloader = new FileDownloader(PathResolver.Downloads);

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

            OnDownloaded(model);
        }

        /// <summary>
        /// Launch file and user select dialog. Send if success.
        /// </summary>
        /// <returns></returns>
        public async Task Send()
        {
            UserModel target = await FileSendPlatformDialogs.SelectUserAsync();

            if (target == null)
            {
                return;
            }

            Models.FileInfo fileInfo = await FileSendPlatformDialogs.SelectFileAsync();

            if (fileInfo == null)
            {
                return;
            }

            if (fileInfo.Name.Length >= 300)
            {
                return;
            }

            HistoryViewModel historyModel = HistoryModelAdd(fileInfo.Name, fileInfo.Content.Length, target);
            FileItem uploadedFile = await UploadFile(fileInfo, historyModel);
            await SendFile(target, uploadedFile, historyModel);
        }

        /// <summary>
        /// Resend specific file. Launches user select dialog.
        /// </summary>
        /// <param name="fileItem">File to send.</param>
        /// <returns></returns>
        public async Task Send(FileItem fileItem)
        {
            UserModel target = await FileSendPlatformDialogs.SelectUserAsync();

            if (target == null)
            {
                return;
            }

            HistoryViewModel historyModel = HistoryModelAdd(fileItem, target);
            await SendFile(target, fileItem, historyModel);
        }

        HistoryViewModel HistoryModelAdd(string filename, long size, UserModel target)
        {
            HistoryViewModel downloadModel = new HistoryViewModel
            {
                Name = filename,
                Status = HistoryModelStatus.Uploading,
                ETA = "",
                Receiver = target.Id,
                Sender = ApiServer.AccountDetails.Id,
                Fake = true,
                Size = size,
                Date = DateTime.Now
            };

            HistoryListViewModel.List.Insert(0, downloadModel);

            return downloadModel;
        }

        HistoryViewModel HistoryModelAdd(FileItem uploadedFile, UserModel target)
        {
            return HistoryModelAdd(uploadedFile.Name, uploadedFile.Size, target);
        }

        /// <summary>
        /// Uploads file to API server.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="downloadModel"></param>
        /// <returns></returns>
        async Task<FileItem> UploadFile(Models.FileInfo fileInfo, HistoryViewModel downloadModel)
        {
            FileUploader fileUploader = new FileUploader();
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

        /// <summary>
        /// Send user file via API.
        /// </summary>
        /// <param name="target">Target user.</param>
        /// <param name="uploadedFile">File to send.</param>
        /// <param name="downloadModel">Model to report progress.</param>
        /// <returns></returns>
        private async Task SendFile(UserModel target, FileItem uploadedFile, HistoryViewModel downloadModel)
        {
            int download_index = await ApiServer.Send(uploadedFile, target.Id);
            downloadModel.Id = download_index;

            downloadModel.Status = HistoryModelStatus.Awaiting;
        }
    }
}
