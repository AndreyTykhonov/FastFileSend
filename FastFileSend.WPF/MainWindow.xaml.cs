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
        DownloadViewModel DownloadViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            DownloadViewModel = new DownloadViewModel();
            ListViewHistory.DataContext = DownloadViewModel;
        }

        private async void ButtonFakeDownload_Click(object sender, RoutedEventArgs e)
        {
            DownloadModel downloadModel = new DownloadModel
            {
                Name = "FirstTest",
                Status = "Time to get serious",
                ETA = "beskonechnost",
                Id = 1337
            };

            DownloadViewModel.DownloadList.Add(downloadModel);

            IFileUploader fileUploader = new FexFileUploader();
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
