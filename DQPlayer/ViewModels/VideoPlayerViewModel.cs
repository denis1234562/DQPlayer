using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace DQPlayer.ViewModels
{
    public class VideoPlayerViewModel : ViewModelBase
    {
        private TimeSpan _movieElapsedTime = default(TimeSpan);
        public TimeSpan MovieElapsedTime
        {
            get => _movieElapsedTime;
            set
            {
                if (value != _movieElapsedTime)
                {
                    _movieElapsedTime = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}