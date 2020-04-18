using FastFileSend.Main.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using FastFileSend.Main.Enum;

namespace FastFileSend.WPF.Controls
{
    class SubStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            HistoryViewModel history = (HistoryViewModel)value;
            string status = SizeHelper.BytesToString(history.File.Size);

            string subStatus = string.Empty;
            switch (history.Status)
            {
                case HistoryModelStatus.Archiving:
                    subStatus = "Archiving";
                    break;
                case HistoryModelStatus.Awaiting:
                    subStatus = "Awaiting remote download";
                    break;
                case HistoryModelStatus.Downloading:
                    subStatus = $"{history.Progress * 100:0}%, {history.ETA}";
                    break;
                case HistoryModelStatus.Unpacking:
                    subStatus = "Unpacking";
                    break;
                case HistoryModelStatus.Uploading:
                    subStatus = $"{history.Progress * 100:0}%, {history.ETA}";
                    break;
                case HistoryModelStatus.UsingAPI:
                    subStatus = "Using API";
                    break;
            }

            if (!string.IsNullOrEmpty(subStatus))
            {
                status += $" ({subStatus})";
            }

            return status;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
