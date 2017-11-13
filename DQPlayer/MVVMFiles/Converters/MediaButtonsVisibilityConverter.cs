using System;
using System.Windows;
using System.Windows.Data;
using DQPlayer.MVVMFiles.Models.MediaPlayer;
using DQPlayer.States;

namespace DQPlayer.MVVMFiles.Converters
{
    [ValueConversion(typeof(MediaPlayerModel), typeof(Visibility))]
    public class MediaButtonsVisibilityConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            //parameter indicates the state player will be set
            //play - true
            //pause - false
            
            if (value == null)
            {
                return Visibility.Hidden;
            }
            var running = ((MediaPlayerModel)value).CurrentState.IsRunning;
            if (running ^ (bool)parameter)
            {
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return Visibility.Hidden;
        }
    }
}