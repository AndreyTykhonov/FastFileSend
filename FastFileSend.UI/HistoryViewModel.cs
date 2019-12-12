using FastFileSend.Main;
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

        HistoryViewModelUpdater HistoryViewModelUpdater { get; set; }

        public HistoryViewModel(ApiServer apiServer)
        {
            List = new ObservableCollection<HistoryModel> { };
            HistoryViewModelUpdater = new HistoryViewModelUpdater(apiServer, this);
        }
    }
}
