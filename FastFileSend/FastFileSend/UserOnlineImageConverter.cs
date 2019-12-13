using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using Xamarin.Forms;

namespace FastFileSend
{
    public class UserOnlineImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool online = ((bool)value);
            var image = online ? UserStatusImage.Online : UserStatusImage.Offline;
            return ImageSource.FromStream(() => new MemoryStream(image));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
