using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using DQPlayer.Annotations;

namespace DQPlayer
{
    public class MoviePlayerSourceTracker : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly MediaElement _moviePlayer;

        private Uri _moviePlayerSource;
        public Uri MoviePlayerSource
        {
            get => _moviePlayerSource;
            set
            {
                if (value != _moviePlayerSource)
                {
                    _moviePlayerSource = value;
                    OnPropertyChanged(nameof(MoviePlayerSource));
                }
            }
        }

        public MoviePlayerSourceTracker(MediaElement moviePlayer)
        {
            _moviePlayer = moviePlayer;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(_moviePlayer, new PropertyChangedEventArgs(propertyName));
        }
    }
}