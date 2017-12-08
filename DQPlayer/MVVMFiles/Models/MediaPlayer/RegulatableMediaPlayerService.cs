using System;
using System.Windows.Controls;
using DQPlayer.Helpers.Extensions;

namespace DQPlayer.MVVMFiles.Models.MediaPlayer
{
    public class RegulatableMediaPlayerService : IRegulatableMediaService, IRegulatableMediaServiceNotifier
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

        public void Play()
        {
            _mediaElement.Play();
            SetTimer();
            OnMediaPlayed();
        }

        public void Pause()
        {
            _mediaElement.Pause();
            SetTimer();
            OnMediaPaused();
        }

        public void Stop()
        {
            _mediaElement.Stop();
            SetTimer();
            _media.MediaSlider.Value = new TimeSpan(0);
            OnMediaStopped();
        }

        public void Rewind()
        {
            var position = GeneralExtensions.Max(_mediaElement.Position.Subtract(Settings.SkipSeconds),
                new TimeSpan(0));
            SetNewPlayerPosition(position);
            OnMediaRewinded(position);
        }

        public void FastForward()
        {
            var position = GeneralExtensions.Min(_mediaElement.NaturalDuration.TimeSpan,
                _mediaElement.Position.Add(Settings.SkipSeconds));
            Console.WriteLine(position);
            SetNewPlayerPosition(position);
            OnMediaFastForwarded(position);
        }

        public void SetNewPlayerPosition(TimeSpan newPosition)
        {
            _mediaElement.Position = newPosition;
            if (_media.MediaSlider.Value != _mediaElement.Position)
            {
                _media.MediaSlider.Value = _mediaElement.Position;
            }
            //TODO skip to positon
        }

        public void SetNewPlayerSource(Uri source)
        {
            _mediaElement.Source = source;
            _media.MediaSlider.Value = new TimeSpan(0);
        }

        #region Implementation of IMediaServiceNotifier

        public event Action<object> MediaPlayed;
        public event Action<object> MediaPaused;
        public event Action<object> MediaStopped;

        private void OnMediaPlayed()
        {
            MediaPlayed?.Invoke(_mediaElement);
        }

        private void OnMediaPaused()
        {
            MediaPaused?.Invoke(_mediaElement);
        }

        private void OnMediaStopped()
        {
            MediaStopped?.Invoke(_mediaElement);
        }
        #endregion

        #region Implementation of IRegulatableMediaServiceNotifier

        public event Action<object, TimeSpan> MediaRewinded;
        public event Action<object, TimeSpan> MediaFastForwarded;

        private void OnMediaRewinded(TimeSpan time)
        {
            MediaRewinded?.Invoke(_mediaElement, time);
        }

        private void OnMediaFastForwarded(TimeSpan time)
        {
            MediaFastForwarded?.Invoke(_mediaElement, time);
        }
        #endregion
    }
}