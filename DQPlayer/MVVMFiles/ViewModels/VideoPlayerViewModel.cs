using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DQPlayer.MVVMFiles.Commands;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public class VideoPlayerViewModel : ViewModelBase
    {
        private TimeSpan _movieElapsedTime = default(TimeSpan);
        /// <summary>
        /// Keeps track of current time of the media.
        /// </summary>
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

        private TimeSpan _movieLeftTime = default(TimeSpan);
        /// <summary>
        /// Keeps track of how much time is left from the media.
        /// </summary>
        public TimeSpan MovieLeftTime
        {
            get => _movieLeftTime;
            set
            {
                if (value != _movieLeftTime)
                {
                    _movieLeftTime = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}