using FastFileSend.Main;
using FastFileSend.Main.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FastFileSend.WPF
{
    class StatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            HistoryModelStatus status = (HistoryModelStatus)value;
            switch (status)
            {
                case HistoryModelStatus.Awaiting:
                    return "Awaiting remote download";
                case HistoryModelStatus.Downloading:
                    return "Downloading";
                case HistoryModelStatus.Ok:
                    return "OK";
                case HistoryModelStatus.Uploading:
                    return "Uploading";
                case HistoryModelStatus.UsingAPI:
                    return "Using API";
                case HistoryModelStatus.Archiving:
                    return "Archiving";
                default:
                    return "Unknown";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
