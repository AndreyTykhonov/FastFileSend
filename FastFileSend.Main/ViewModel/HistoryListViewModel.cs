using FastFileSend.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FastFileSend.Main.ViewModel
{
    /// <summary>
    /// Represents list of History Model.
    /// </summary>
    public class HistoryListViewModel
    {
        public ObservableCollection<HistoryViewModel> List { get; private set; } = new ObservableCollection<HistoryViewModel> { };

        public HistoryViewModel Selected { get; set; }
    }
}
