using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Controls;
using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.Extensions;
using DQPlayer.MVVMFiles.Models.MediaPlayer;

namespace DQPlayer.Helpers.SubtitlesManagement
{
    public class SubtitleHandler
    {
        public event Action<SubtitleHandler, SubtitleSegment> DisplaySubtitle;
        public event Action<SubtitleHandler, SubtitleSegment> HideSubtitle;

        private CancellationTokenSource _cancellationToken;
        private readonly IntermissionTimer _subtitleDisplayTracker = new IntermissionTimer();

        private CircularList<SubtitleSegment> _subtitles;
        private readonly HashSet<SubtitleSegment> _currentlyShownSubtitles = new HashSet<SubtitleSegment>();

        private bool _isStartable = true;
        private Encoding _encoding;

        public SubtitleHandler WithEncoding(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            return this;
        }
        public SubtitleHandler IsStartable(bool start)
        {
            _isStartable = start;
            return this;
        }
        public SubtitleHandler Build(string path, TimeSpan currentTime, IRegulatableMediaServiceNotifier notifier)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (notifier == null)
            {
                throw new ArgumentNullException(nameof(notifier));
            }
            _subtitles = new SubtitleReader(_encoding).ExtractSubtitles(path);
            _cancellationToken = new CancellationTokenSource();
            SetupTimer(currentTime, _isStartable);
            SetupNotifierSubscriptions(notifier);
            //TODO might need to reset values
            return this;
        }

        private void SetupTimer(TimeSpan currentTime, bool start)
        {
            _subtitleDisplayTracker.Tick += OnSubtitleDisplayTrackerTick;
            ResyncSubtitles(currentTime);
            if (start)
            {
                _subtitleDisplayTracker.Start();
            }
        }
        private void SetupNotifierSubscriptions(IRegulatableMediaServiceNotifier notifier)
        {
            notifier.MediaFastForwarded += (o, span) => ResyncSubtitles(span);
            notifier.MediaRewinded += (o, span) => ResyncSubtitles(span);
            notifier.MediaPlayed += OnMediaPlayed;
            notifier.MediaPaused += OnMediaPaused;
            notifier.MediaStopped += OnMediaStopped;
        }

        private void OnMediaStopped(object sender)
        {
            Pause();
            ResyncSubtitles(new TimeSpan(0));
        }

        private void OnMediaPlayed(object sender)
        {
            var mediaElement = sender as MediaElement;
            Resume(mediaElement.Position);
        }

        private void OnMediaPaused(object sender)
        {
            Pause();
        }

        private void Pause()
        {
            _subtitleDisplayTracker.Pause();
            _cancellationToken.Cancel();
        }

        private void Resume(TimeSpan span)
        {
            ScheduleSubtitleHidingAll(span);
            _subtitleDisplayTracker.Resume();
        }

        private void OnSubtitleDisplayTrackerTick(object sender, EventArgs eventArgs)
        {
            var lastSub = _subtitles.Current;
            ShowSubtitle(lastSub, lastSub.SubtitleInterval.Duration);

            _subtitles.MoveNext();
            _subtitleDisplayTracker.Interval = _subtitles.Current.SubtitleInterval.Start - lastSub.SubtitleInterval.Start;
        }

        private void ShowSubtitle(SubtitleSegment subtitle, TimeSpan duration)
        {
            _cancellationToken = new CancellationTokenSource();
            OnDisplaySubtitle(subtitle);
            _currentlyShownSubtitles.Add(subtitle);

            ScheduleSubtitleHiding(subtitle, duration);
        }

        private void ScheduleSubtitleHiding(SubtitleSegment subtitle, TimeSpan interval)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(interval, _cancellationToken.Token);
                    OnHideSubtitle(subtitle);
                    _currentlyShownSubtitles.Remove(subtitle);
                }
                catch (TaskCanceledException) { }
            });
        }

        private void ScheduleSubtitleHidingAll(TimeSpan interval)
        {
            foreach (var subtitle in _currentlyShownSubtitles)
            {
                ScheduleSubtitleHiding(subtitle,
                    GeneralExtensions.Max(new TimeSpan(0), subtitle.SubtitleInterval.End - interval));
            }
        }

        private void ForceHidingAllSubtitles()
        {
            _cancellationToken.Cancel(false);
            lock (_currentlyShownSubtitles)
            {
                foreach (var currentlyShownSubtitle in _currentlyShownSubtitles)
                {
                    OnHideSubtitle(currentlyShownSubtitle);
                }
                _currentlyShownSubtitles.Clear();
            }
        }

        public void ResyncSubtitles(TimeSpan time)
        {
            for (var i = 0; i < _subtitles.Count; i++)
            {
                if (time < _subtitles[i].SubtitleInterval.End)
                {
                    ForceHidingAllSubtitles();
                    ShowSubtitle(_subtitles[i], _subtitles[i].SubtitleInterval.End - time);

                    if (i + 1 < _subtitles.Count)
                    {
                        _subtitleDisplayTracker.Interval = _subtitles[i + 1].SubtitleInterval.Start - time;
                    }
                    //TODO might need to set current to i + 1;
                    _subtitles.SetCurrent(i);
                    break;
                }
            }
        }

        private void OnDisplaySubtitle(SubtitleSegment subtitleToShow)
        {
            DisplaySubtitle?.Invoke(this, subtitleToShow);
        }

        private void OnHideSubtitle(SubtitleSegment subtitleToHide)
        {
            HideSubtitle?.Invoke(this, subtitleToHide);
        }
    }
}
