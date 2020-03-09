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
        #pragma warning disable CS0067 // fody
        public event PropertyChangedEventHandler PropertyChanged;

        public double Progress { get; set; }
        public string ETA { get; set; } = string.Empty;
        public bool Fake { get; set; } = false;

        public override long Size { get; set; }
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
