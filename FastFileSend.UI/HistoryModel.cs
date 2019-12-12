using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace FastFileSend.UI
{
    public class HistoryModel : INotifyPropertyChanged
    {
        private double progress;
        private string status;
        private string eta;

        public string Url { get; set; }
        public double Progress
        {
            get { return progress; }
            set { progress = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get { return status;  }
            set { status = value; OnPropertyChanged(); }
        }

        public string ETA
        {
            get { return eta; }
            set { eta = value; OnPropertyChanged(); }
        }

        public int Id { get; set; }
        
        public long Size { get; set; }

        public string Name { get; set; }

        public int UploadId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
