using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.Extensions;
using DQPlayer.MVVMFiles.ViewModels;

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
        public SubtitleHandler Build(string path, TimeSpan currentTime, MediaPlayerControlsViewModel notifier)
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
        private void SetupNotifierSubscriptions(MediaPlayerControlsViewModel notifier)
        {
            //notifier.MediaPositionChanged += (o, span) => ResyncSubtitles(span);
            //notifier.PlayClick += OnMediaPlayed;
            //notifier.PauseClick += OnMediaPaused;
            //notifier.StopClick += OnMediaStopped;         
        }

        private void OnMediaStopped(object sender)
        {
            Pause();
            ResyncSubtitles(new TimeSpan(0));
        }

        private void OnMediaPlayed(object sender)
        {
            //TODO 1 sec delay
            Resume((TimeSpan) sender);
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
            //TODO 1 sec delay exactly 1 sec
            _cancellationToken = new CancellationTokenSource();
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
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Task cancelled");
                }
            });
        }

        private void ScheduleSubtitleHidingAll(TimeSpan interval)
        {
            foreach (var subtitle in _currentlyShownSubtitles)
            {
                ScheduleSubtitleHiding(subtitle,
                    GeneralExtensions.Max(new TimeSpan(0), subtitle.SubtitleInterval.End.Subtract(interval)));
            }
        }

        public void ForceHidingAllSubtitles()
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
                    if (time > _subtitles[i].SubtitleInterval.Start)
                    {
                        ShowSubtitle(_subtitles[i], _subtitles[i].SubtitleInterval.End - time);
                        if (i + 1 < _subtitles.Count)
                        {
                            _subtitleDisplayTracker.Interval = _subtitles[i + 1].SubtitleInterval.Start - time;
                        }
                        _subtitles.SetCurrent(i + 1);
                        break;
                    }
                    _subtitleDisplayTracker.Interval = _subtitles[i].SubtitleInterval.Start - time;
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
