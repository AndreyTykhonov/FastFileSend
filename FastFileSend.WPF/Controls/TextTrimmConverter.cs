using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FastFileSend.WPF.Controls
{
    class TextTrimmConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const int maxTextLength = 40;

            string text = (string)value;
            if (text.Length > maxTextLength)
            {
                text = string.Join("", text.Take(maxTextLength));
                text += "...";
            }
            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
