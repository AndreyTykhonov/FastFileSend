using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FastFileSend.UI
{
    public class HistoryViewModel
    {
        public ObservableCollection<HistoryModel> List { get; private set; }
        public HistoryModel Selected { get; set; }

        public HistoryViewModel()
        {
            List = new ObservableCollection<HistoryModel> { };
        }
    }
}
