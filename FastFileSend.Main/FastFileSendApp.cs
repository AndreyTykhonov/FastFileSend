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
        public UserListViewModel UserListViewModel { get; private set; }

        private Api ApiServer { get; set; }

        public AccountDetails AccountDetails { get => ApiServer.AccountDetails; }

        #pragma warning disable IDE0052
        private HistoryViewModelUpdater HistoryViewModelUpdater { get; set; }
        private UserListViewModelUpdater UserListViewModelUpdateStatus { get; set; }

        private IPathResolver PathResolver { get; set; }
        private IFastFileSendPlatformDialogs FileSendPlatformDialogs { get; set; }

        private FastFileSendApp(IPathResolver pathResolver, IFastFileSendPlatformDialogs fileSendPlatformDialogs, Api api)
        {
            PathResolver = pathResolver;
            FileSendPlatformDialogs = fileSendPlatformDialogs;

            ApiServer = api;

            HistoryListViewModel = new HistoryListViewModel();
            UserListViewModel = new UserListViewModel(ApiServer, PathResolver.UsersConfig);

            HistoryListViewModel.List.CollectionChanged += HistoryListView_CollectionChanged;

            HistoryViewModelUpdater = new HistoryViewModelUpdater(ApiServer, HistoryListViewModel);
            UserListViewModelUpdateStatus = new UserListViewModelUpdater(ApiServer, UserListViewModel);
        }

        /// <summary>
        /// This class should have async constructor, so you need to use this method.
        /// </summary>
        /// <param name="pathResolver">Platform path resolver.</param>
        /// <param name="fileSendPlatformDialogs">Platform dialogs resolver.</param>
        /// <returns></returns>
        public static async Task<FastFileSendApp> Create(IPathResolver pathResolver, IFastFileSendPlatformDialogs fileSendPlatformDialogs)
        {
            if (pathResolver is null)
            {
                throw new ArgumentNullException(nameof(pathResolver));
            }

            if (fileSendPlatformDialogs is null)
            {
                throw new ArgumentNullException(nameof(fileSendPlatformDialogs));
            }

            Api api;
            if (File.Exists(pathResolver.AccountConfig))
            {
                AccountDetails accountDetails = JsonConvert.DeserializeObject<AccountDetails>(File.ReadAllText(pathResolver.AccountConfig));
                api = await Api.Login(accountDetails).ConfigureAwait(true);
            }
            else
            {
                api = await Api.CreateNewAccount().ConfigureAwait(true);
                string json = JsonConvert.SerializeObject(api.AccountDetails);
                File.WriteAllText(pathResolver.AccountConfig, json);
            }

            return new FastFileSendApp(pathResolver, fileSendPlatformDialogs, api);
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

            await Download(model).ConfigureAwait(false);
        }

        private async Task Download(HistoryViewModel model)
        {
            FileDownloader fileDownloader = new FileDownloader(PathResolver.Downloads);

            FileItem fileItem = model.File;

            model.Status = HistoryModelStatus.Downloading;

            fileDownloader.OnProgress += (double progress, double speed) =>
            {
                model.Progress = progress;
                model.ETA = SizeUtils.BytesToString(Convert.ToInt32(speed), "/s");
            };

            await fileDownloader.DownloadAsync(fileItem).ConfigureAwait(false);

            model.Status = HistoryModelStatus.Ok;
            model.ETA = "";

            await ApiServer.NotifyDownloadedAsync(model.Id).ConfigureAwait(false);
        }

        /// <summary>
        /// Launch file and user select dialog. Send if success.
        /// </summary>
        /// <returns></returns>
        public async Task Send()
        {
            UserModel target = await FileSendPlatformDialogs.SelectUserAsync(UserListViewModel).ConfigureAwait(false);

            if (target == null)
            {
                return;
            }

            Models.FileInfo fileInfo = await FileSendPlatformDialogs.SelectFileAsync().ConfigureAwait(false);

            if (fileInfo == null)
            {
                return;
            }

            if (fileInfo.Name.Length >= 300)
            {
                return;
            }

            HistoryViewModel historyModel = HistoryModelAdd(fileInfo.Name, fileInfo.Content.Length, target);
            FileItem uploadedFile = await UploadFile(fileInfo, historyModel).ConfigureAwait(false);
            await SendFile(target, uploadedFile, historyModel).ConfigureAwait(false);
        }

        /// <summary>
        /// Resend specific file. Launches user select dialog.
        /// </summary>
        /// <param name="fileItem">File to send.</param>
        /// <returns></returns>
        public async Task Send(FileItem fileItem)
        {
            if (fileItem is null)
            {
                throw new ArgumentNullException(nameof(fileItem));
            }

            UserModel target = await FileSendPlatformDialogs.SelectUserAsync(UserListViewModel).ConfigureAwait(false);

            if (target == null)
            {
                return;
            }

            HistoryViewModel historyModel = HistoryModelAdd(fileItem, target);
            await SendFile(target, fileItem, historyModel).ConfigureAwait(false);
        }

        HistoryViewModel HistoryModelAdd(string filename, long size, UserModel target)
        {
            HistoryViewModel downloadModel = new HistoryViewModel
            {
                File = new FileItem(0, filename, size, 0, DateTime.Now, null),
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

            FileItem fileItem = await fileUploader.UploadAsync(fileInfo.Name, fileInfo.Content).ConfigureAwait(false);

            downloadModel.Progress = 100;
            downloadModel.Status = HistoryModelStatus.UsingAPI;

            return await ApiServer.Upload(fileItem).ConfigureAwait(false);
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
            int download_index = await ApiServer.Send(uploadedFile, target.Id).ConfigureAwait(false);
            downloadModel.Id = download_index;

            downloadModel.Status = HistoryModelStatus.Awaiting;
            downloadModel.Progress = 100;
        }
    }
}
