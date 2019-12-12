using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FastFileSend.UI;
using FastFileSend.Main;
using System.IO;

namespace FastFileSend.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        HistoryViewModel HistoryViewModel { get; set; }
        UserViewModel UserViewModel { get; set; }
        ApiServer ApiServer { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            IsEnabled = false;

            AuthUser();
        }

        private async Task AuthUser()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.password))
            {
                ApiServer = await ApiServer.CreateNewAccount();

                Properties.Settings.Default.id = ApiServer.Id;
                Properties.Settings.Default.password = ApiServer.Password;
                Properties.Settings.Default.Save();
            }
            else
            {
                ApiServer = new ApiServer(Properties.Settings.Default.id, Properties.Settings.Default.password);
            }

            TextBlockId.Text = ApiServer.Id.ToString();


            HistoryViewModel = new HistoryViewModel(ApiServer);
            ListViewHistory.DataContext = HistoryViewModel;

            UserViewModel = new UserViewModel(ApiServer);

            IsEnabled = true;
        }

        UserModel SelectUser()
        {
            UsersWindow usersWindow = new UsersWindow(UserViewModel);
            usersWindow.ShowDialog();

            return UserViewModel.Selected;
        }

        private async void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            UserModel target = SelectUser();

            if (target == null)
            {
                return;
            }

            await SendFile(target, @"C:\Users\KoBRa\Downloads\MahApps.Metro.Demo-v1.6.5-rc0001.zip");
        }

        private async Task SendFile(UserModel target, string path)
        {
            HistoryModel downloadModel = new HistoryModel
            {
                Name = System.IO.Path.GetFileName(path),
                StatusText = "Uploading file",
                ETA = "",
                Receiver = target.Id,
                Sender = ApiServer.Id
            };

            HistoryViewModel.List.Insert(0, downloadModel);

            IFileUploader fileUploader = new DummyFileUploader();
            fileUploader.OnProgress += (double progress, double speed) =>
            {
                downloadModel.Progress = progress;
                downloadModel.ETA = speed.ToString("0.00 MB/s");
            };

            fileUploader.OnEnd += () =>
            {
                downloadModel.Progress = 100;
                downloadModel.StatusText = "Using API";
            };

            CloudFile cloudFile = await fileUploader.UploadAsync(path);

            cloudFile = new CloudFile(0, "debug.zip", 0, DateTime.Now, "https://cdn.shazoo.ru/393609_KPsmQaHsNk_382993_uuwtofdnti_fb6f81c359f7cd.jpg");

            FileItem uploadedFile = await ApiServer.Upload(cloudFile);
            int download_index = await ApiServer.Send(uploadedFile, target.Id);
            downloadModel.Id = download_index;

            downloadModel.StatusText = "Awaiting remote download";
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
