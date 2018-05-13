using System;
using System.Windows.Data;

namespace DQPlayer.MVVMFiles.Converters
{
    [ValueConversion(typeof(double), typeof(TimeSpan))]
    public class SecondsToTimeSpanConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return TimeSpan.FromSeconds((double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return ((TimeSpan)value).TotalSeconds;
        }
    }
}