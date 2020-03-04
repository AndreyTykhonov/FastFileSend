using FastFileSend.Main.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FastFileSend.Main.ViewModel
{
    public class HistoryViewModel : HistoryModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public double Progress { get; set; }
        public string ETA { get; set; }
        public bool Fake { get; set; } = false;
    }
}
