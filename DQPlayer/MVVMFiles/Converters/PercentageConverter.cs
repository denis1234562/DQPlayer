using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace DQPlayer.MVVMFiles.Converters
{
    public class PercentageConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var simulatedPosition = (double) value;
            var slider = (Slider) parameter;

            return $"{simulatedPosition / slider.Maximum * 100d}%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}