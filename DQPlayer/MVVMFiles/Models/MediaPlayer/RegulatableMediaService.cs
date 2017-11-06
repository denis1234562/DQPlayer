using System;
using System.Windows.Controls;

namespace DQPlayer.MVVMFiles.Models.MediaPlayer
{
    public class RegulatableMediaService : IRegulatableMediaService
    {
        private readonly IRegulatableMediaPlayer _media;
        private readonly MediaElement _mediaElement;

        public RegulatableMediaService(MediaElement mediaElement, IRegulatableMediaPlayer media)
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
        }

        public void Pause()
        {
            _mediaElement.Pause();
            SetTimer();
        }

        public void Stop()
        {
            _mediaElement.Stop();
            SetTimer();
            _media.MediaSlider.Value = new TimeSpan(0);
        }

        public void Rewind()
        {
            SetNewPlayerPosition(Extensions.Max(_mediaElement.Position.Subtract(Settings.SkipSeconds), new TimeSpan(0)));
        }

        public void FastForward()
        {
            SetNewPlayerPosition(Extensions.Min(_mediaElement.NaturalDuration.TimeSpan,
                _mediaElement.Position.Add(Settings.SkipSeconds)));
        }

        public void SetNewPlayerPosition(TimeSpan newPosition)
        {
            _mediaElement.Position = newPosition;
            _media.MediaSlider.Value = newPosition;
        }
    }
}