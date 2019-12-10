using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FastFileSend.UI
{
    public class DownloadViewModel
    {
        public ObservableCollection<DownloadModel> DownloadList { get; private set; }
        public DownloadModel DownloadSelected { get; set; }

        public DownloadViewModel()
        {
            DownloadList = new ObservableCollection<DownloadModel> { };
        }
    }
}
