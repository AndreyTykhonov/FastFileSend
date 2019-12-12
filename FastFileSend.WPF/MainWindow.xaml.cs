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

namespace FastFileSend.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        HistoryViewModel HistoryViewModel { get; set; }
        ApiServer ApiServer { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            HistoryViewModel = new HistoryViewModel();
            ListViewHistory.DataContext = HistoryViewModel;

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

            IsEnabled = true;
        }

        UserModel SelectUser()
        {
            UserViewModel userViewModel = new UserViewModel();
            UsersWindow usersWindow = new UsersWindow(userViewModel);
            usersWindow.ShowDialog();

            return userViewModel.Selected;
        }

        private async void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            SelectUser();

            return;

            HistoryModel downloadModel = new HistoryModel
            {
                Name = "FirstTest",
                Status = "Time to get serious",
                ETA = "beskonechnost",
                Id = 1337
            };

            HistoryViewModel.List.Add(downloadModel);

            IFileUploader fileUploader = new DummyFileUploader();
            fileUploader.OnProgress += (double progress, double speed) =>
            {
                downloadModel.Progress = progress;
                downloadModel.ETA = speed.ToString("0.00 MB/s");
            };

            fileUploader.OnEnd += () =>
            {
                downloadModel.Progress = 100;
                downloadModel.Status = "FINISHED";

            };

            CloudFile cloudFile = await fileUploader.UploadAsync(@"C:\Users\KoBRa\Downloads\MahApps.Metro.Demo-v1.6.5-rc0001.zip");
            Clipboard.SetText(cloudFile.Url);
        }
    }
}
