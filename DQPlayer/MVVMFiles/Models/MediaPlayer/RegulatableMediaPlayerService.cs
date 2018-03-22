using System;
using System.Windows.Controls;
using DQPlayer.Helpers.Extensions;

namespace DQPlayer.MVVMFiles.Models.MediaPlayer
{
    public class RegulatableMediaPlayerService : IRegulatableMediaService
    {
        private readonly IRegulatableMediaPlayer _media;
        private readonly MediaElement _mediaElement;

        public RegulatableMediaPlayerService(MediaElement mediaElement, IRegulatableMediaPlayer media)
        {
            _media = media;
            _mediaElement = mediaElement;
        }

        private void SetTimer()
        {
            _media.MediaPlayerTimer.Pause();
            if (_media.CurrentState.IsRunning)
            {
                _media.MediaPlayerTimer.Resume();
            }
        }

        #region Implementation of IMediaService

        void IMediaService.Play()
        {
            _mediaElement.Play();
            SetTimer();
        }

        void IMediaService.Pause()
        {
            _mediaElement.Pause();
            SetTimer();
        }

        void IMediaService.Stop()
        {
            _mediaElement.Stop();
            SetTimer();
        }

        public void ChangeVolume(double value)
        {
            _mediaElement.Volume = value;
        }

        #endregion

        #region Implementation of IRegulatableMediaService

        void IRegulatableMediaService.Rewind()
        {
            var position = GeneralExtensions.Max(_mediaElement.Position.Subtract(Settings.SkipSeconds),
                new TimeSpan(0));
            SetNewPlayerPosition(position);
        }

        void IRegulatableMediaService.FastForward()
        {
            var position = GeneralExtensions.Min(_mediaElement.NaturalDuration.TimeSpan,
                _mediaElement.Position.Add(Settings.SkipSeconds));
            SetNewPlayerPosition(position);
        }

        public void SetNewPlayerPosition(TimeSpan newPosition)
        {
            _mediaElement.Position = newPosition;
        }

        public void SetNewPlayerSource(Uri source)
        {
            _mediaElement.Source = source;
            _mediaElement.Position = new TimeSpan(0);
        }

        #endregion
    }
}