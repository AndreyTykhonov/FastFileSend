using FastFileSend.Main.Enum;
using FastFileSend.Main.Interfaces;
using FastFileSend.Main.Models;
using FastFileSend.Main.RemoteFile;
using FastFileSend.Main.Utils;
using FastFileSend.Main.ViewModel;
using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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
            FileItem file = model.File;

            string filePath = FindEmptyPath(PathResolver.Downloads, file.Name);

            if (model.File.Folder)
            {
                filePath = Path.Combine(PathResolver.Temp, file.Name);
            }

            model.Status = HistoryModelStatus.Downloading;

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                FileDownloader downloader = new FileDownloader();

                if (file.Url.Count > 1)
                {
                    int segmentsDownloaded = 0;
                    double segmentProgress = 1f / file.Url.Count;
                    downloader.OnProgress += (double progress, double speed) =>
                    {
                        double uploadedProgress = segmentsDownloaded * segmentProgress;
                        model.Progress = uploadedProgress + (segmentProgress * progress);
                        model.ETA = SizeUtils.BytesToString(Convert.ToInt32(speed), "/s");
                    };

                    fs.SetLength(file.Size);
                    for (int i = 0; i < file.Url.Count; i++)
                    {
                        long segmentPosition = i * Settings.FileSegmentSize;
                        long segmentLength = (long)Math.Min(file.Size - segmentPosition, Settings.FileSegmentSize);
                        using (SegmentedStream segmented = new SegmentedStream(fs, segmentPosition, segmentLength))
                        {
                            await downloader.DownloadAsync(segmented, file.Url[i], segmentLength).ConfigureAwait(false);
                        }
                        segmentsDownloaded++;
                    }
                }
                else
                {
                    downloader.OnProgress += (double progress, double speed) =>
                    {
                        model.Progress = progress;
                        model.ETA = SizeUtils.BytesToString(Convert.ToInt32(speed), "/s");
                    };

                    await downloader.DownloadAsync(fs, file.Url.First(), file.Size).ConfigureAwait(false);
                }

                if (file.Folder)
                {
                    model.Status = HistoryModelStatus.Unpacking;
                    using (ZipFile zip = new ZipFile(filePath))
                    {
                        string unpackPath = Path.Combine(PathResolver.Downloads, file.Name);
                        unpackPath = FindEmptyFolder(unpackPath);

                        zip.ExtractProgress += (object sender, ExtractProgressEventArgs e) =>
                        {
                            if (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry)
                            {
                                //Debug.WriteLine(e.CurrentEntry.FileName);
                                model.Progress = (double)e.EntriesExtracted / e.EntriesTotal;
                            }
                        };

                        await zip.ExtractAllAsync(unpackPath).ConfigureAwait(false);
                    }

                    fs.Close();
                    File.Delete(filePath);
                }

                model.Status = HistoryModelStatus.Ok;
                model.ETA = "";

                await ApiServer.NotifyDownloadedAsync(model.Id).ConfigureAwait(false);
            }
        }

        string FindEmptyFolder(string folder)
        {
            for (int i = 1; i < 100; i++)
            {
                string name = Path.GetFileName(folder);
                folder = Path.GetDirectoryName(folder);

                string path = Path.Combine(folder, $"{name} ({i})");
                if (!Directory.Exists(path))
                {
                    return path;
                }
            }

            #pragma warning disable CA1303
            throw new IOException("100 duplicates?!");
        }

        string FindEmptyPath(string folder, string filename)
        {
            for (int i = 1; i < 100; i++)
            {
                string name = Path.GetFileNameWithoutExtension(filename);
                string ext = Path.GetExtension(filename);

                string path = Path.Combine(folder, $"{name} ({i}){ext}");
                if (!File.Exists(path))
                {
                    return path;
                }
            }

            #pragma warning disable CA1303
            throw new IOException("100 duplicates?!");
        }

        public async Task SendFolder(string folder = "")
        {
            UserModel receiver = await FileSendPlatformDialogs.SelectUserAsync(UserListViewModel).ConfigureAwait(false);

            if (receiver == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(folder))
            {
                folder = await FileSendPlatformDialogs.SelectFolderAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(folder))
                {
                    return;
                }
            }

            HistoryViewModel historyModel = HistoryModelAdd(Path.GetFileName(folder), 0, receiver);

            string zipPath = Path.Combine(PathResolver.Temp, Path.GetFileName(folder));

            using (ZipFile zip = new ZipFile())
            {
                zip.AlternateEncodingUsage = ZipOption.Always;
                zip.AlternateEncoding = Encoding.UTF8;
                zip.UseZip64WhenSaving = Zip64Option.Always;

                zip.AddDirectory(folder);

                historyModel.Status = HistoryModelStatus.Archiving;
                zip.SaveProgress += (object sender, SaveProgressEventArgs e) =>
                {
                    if (e.EventType == ZipProgressEventType.Saving_AfterWriteEntry)
                    {
                        historyModel.Progress = (double)e.EntriesSaved / e.EntriesTotal;
                    }
                };

                await zip.SaveAsync(zipPath).ConfigureAwait(false);
            }

            using (FileStream fs = new FileStream(zipPath, FileMode.Open))
            {
                Models.FileInfo fileInfo = new Models.FileInfo { Name = Path.GetFileName(zipPath), Content = fs, Folder = true };
                historyModel.Size = fs.Length;
                historyModel.Status = HistoryModelStatus.Uploading;

                FileItem uploadedFile = await UploadFile(fileInfo, historyModel).ConfigureAwait(false);
                uploadedFile.Folder = true;
                await SendFile(receiver, uploadedFile, historyModel).ConfigureAwait(false);
            }
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
        /// Launch user select dialog. Send if success. File path as input.
        /// </summary>
        /// <returns></returns>
        public async Task Send(string filePath)
        {
            UserModel target = await FileSendPlatformDialogs.SelectUserAsync(UserListViewModel).ConfigureAwait(false);

            if (target == null)
            {
                return;
            }

            FileStream fs = new FileStream(filePath, FileMode.Open);
            Models.FileInfo fileInfo = new Models.FileInfo
            {
                Name = Path.GetFileName(filePath),
                Content = fs
            };

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
            long size = fileInfo.Content.Length;
            if (size > Settings.FileSegmentSize)
            {
                return await UploadFileSegmented(fileInfo, downloadModel).ConfigureAwait(false);
            }

            FileUploader fileUploader = new FileUploader();
            fileUploader.OnProgress += (double progress, double speed) =>
            {
                downloadModel.Progress = progress;
                downloadModel.ETA = SizeUtils.BytesToString(Convert.ToInt32(speed), "/s");
            };

            FileItem fileItem = await fileUploader.UploadAsync(fileInfo.Name, fileInfo.Content).ConfigureAwait(false);
            fileItem.Folder = fileInfo.Folder;

            downloadModel.Progress = 100;
            downloadModel.Status = HistoryModelStatus.UsingAPI;

            return await ApiServer.Upload(fileItem).ConfigureAwait(false);
        }

        /// <summary>
        /// Create file segments and upload it.
        /// </summary>
        /// <param name="fileInfo">File to send.</param>
        /// <param name="downloadModel">Download model to report.</param>
        /// <returns></returns>
        async Task<FileItem> UploadFileSegmented(Models.FileInfo fileInfo, HistoryViewModel downloadModel)
        {
            FileItem segmentedFile = new FileItem(0, fileInfo.Name, fileInfo.Content.Length, 0, DateTime.Now, null);
            segmentedFile.Folder = fileInfo.Folder;

            int segmentsCount = (int)Math.Ceiling((double)fileInfo.Content.Length / Settings.FileSegmentSize);
            downloadModel.Status = HistoryModelStatus.Uploading;

            double oneSegmentItemProgress = 1f / segmentsCount;

            List<Uri> segmentsUri = new List<Uri>();

            FileUploader fileUploader = new FileUploader();
            fileUploader.OnProgress += (double progress, double speed) =>
            {
                int itemsUploaded = segmentsUri.Count;
                double uploadedProgress = itemsUploaded * oneSegmentItemProgress;
                downloadModel.Progress = uploadedProgress + (oneSegmentItemProgress * progress);
                downloadModel.ETA = SizeUtils.BytesToString(Convert.ToInt32(speed), "/s");
            };

            for (int i = 0; i < segmentsCount; i++)
            {
                using (SegmentedStream segmentedStream = new SegmentedStream(fileInfo.Content, i * Settings.FileSegmentSize, Settings.FileSegmentSize))
                {
                    FileItem uploadedSegment = await fileUploader.UploadAsync(fileInfo.Name, segmentedStream).ConfigureAwait(false);
                    segmentsUri.Add(uploadedSegment.Url.First());
                }
            }

            downloadModel.Status = HistoryModelStatus.UsingAPI;
            downloadModel.ETA = "";

            segmentedFile.Url = segmentsUri;
            return await ApiServer.Upload(segmentedFile).ConfigureAwait(false);
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
