using System;
using System.Windows.Data;
using DQPlayer.Helpers.CustomControls;
using DQPlayer.Helpers.Extensions;

namespace DQPlayer.MVVMFiles.Converters
{
    [ValueConversion(typeof(double), typeof(TimeSpan))]
    public class LeftTimeConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            ThumbDragSlider param = (ThumbDragSlider) parameter;
            return TimeSpan.FromSeconds(param.Maximum).Subtract(TimeSpan.FromSeconds((double)value)).ToShortString();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}