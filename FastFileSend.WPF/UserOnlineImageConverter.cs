using FastFileSend.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace FastFileSend.WPF
{
    public class UserOnlineImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool online = ((bool)value);
            return online ? UserStatusImage.Online : UserStatusImage.Offline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
