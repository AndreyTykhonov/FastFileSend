using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace FastFileSend.UI
{
    public class DownloadModel : INotifyPropertyChanged
    {
        private double progress;
        private string status;
        private string eta;

        public string Url { get; set; }
        public double Progress
        {
            get { return progress; }
            set { OnPropertyChanged(); progress = value; }
        }

        public string Status
        {
            get { return status;  }
            set { OnPropertyChanged(); status = value; }
        }

        public string ETA
        {
            get { return eta; }
            set { OnPropertyChanged(); eta = value; }
        }

        public int Id { get; set; }
        
        public long Size { get; set; }

        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
