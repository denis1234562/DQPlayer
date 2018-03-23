using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.FileManagement.FileInformation;
using System.Windows.Data;
using System;
using System.Globalization;
using System.Linq;

namespace DQPlayer.MVVMFiles.Converters
{
    [ValueConversion(typeof(ObservableCircularList<MediaFileInformation>), typeof(string))]
    public class PlaylistFilesDurationConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var files = (ObservableCircularList<MediaFileInformation>)value;
            TimeSpan duration = new TimeSpan(files.Sum(f => f.FileLength.Ticks));
            return $"Playlist time: {new TimeSpanFormatConverter().Convert(duration, typeof(string), null, culture)}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
