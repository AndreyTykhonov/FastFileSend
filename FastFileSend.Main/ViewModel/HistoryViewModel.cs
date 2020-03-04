using FastFileSend.Main.Enum;
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
        public string ETA { get; set; } = string.Empty;
        public bool Fake { get; set; } = false;

        public override HistoryModelStatus Status { get; set; }

        public HistoryViewModel(HistoryModel historyModel)
        {
            DynamicMapper.Map(historyModel, this);
        }

        public HistoryViewModel()
        {
            // empty
        }
    }
}
