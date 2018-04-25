using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using DQPlayer.Helpers.Extensions;

namespace DQPlayer.MVVMFiles.Converters
{
    public class SliderTooltipValueConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var slider = value as Slider;
            var converter = parameter as IValueConverter;
            if (slider == null) throw new InvalidOperationException($"The target must be of type {nameof(Slider)}");
            if (converter == null) throw new InvalidOperationException($"The parameter must be of type {nameof(IValueConverter)}");

            return new Func<object>(() =>
            {
                var track = slider.GetElementFromTemplate<Track>("PART_Track");
                if (track == null)
                {
                    return null;
                }
                double simulatedPosition = track.SimulateTrackPosition(Mouse.GetPosition(slider));
                return converter.Convert(simulatedPosition, typeof(string), slider, null);
            });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}