using System;
using System.Windows.Data;
using DQPlayer.Helpers.Extensions;

namespace DQPlayer.MVVMFiles.Converters
{
    [ValueConversion(typeof(TimeSpan), typeof(string))]
    public class TimeSpanFormatConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string)) throw new InvalidOperationException($"The target must be a {nameof(String)}");

            return ((TimeSpan)value).ToShortString();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return TimeSpan.Parse((string) value);
        }
    }
}
