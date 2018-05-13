using System;
using System.Linq;
using System.Text;
using DQPlayer.States;
using System.Threading;
using DQPlayer.Annotations;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using DQPlayer.MVVMFiles.ViewModels;
using DQPlayer.Helpers.InputManagement;
using Microsoft.VisualStudio.Threading;
using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.Extensions;
using DQPlayer.Helpers.Extensions.CollectionsExtensions;
using DQPlayer.Helpers.MediaEnumerations;
using DQPlayer.Helpers.FileManagement.FileInformation;

namespace DQPlayer.Helpers.SubtitlesManagement
{
    public sealed class SubtitleHandler : ICustomObservable<MediaEventArgs<SubtitlesEventType>>
    {
        private CancellationTokenSource _forceHidingToken = new CancellationTokenSource();
        private CancellationTokenSource _pauseToken = new CancellationTokenSource();
        private readonly AsyncManualResetEvent _manualResetEventAsync = new AsyncManualResetEvent();

        private CircularList<SubtitleSegment> _subtitles;

        private readonly HashSet<SubtitleSegment> _currentlyShownSubtitles =
            new HashSet<SubtitleSegment>(new ReferenceComparer<SubtitleSegment>());

        private readonly IntermissionTimer _subtitleDisplayTracker;

        private readonly Encoding _encoding;

        private IMediaElementViewModel _providerMediaElement;

        private MediaObservableMap<SubscriptionEventType> _subtitlesProviderMap;
        private MediaObservableMap<MediaElementEventType> _mediaElementProviderMap;
        private MediaObservableMap<MediaControlEventType> _mediaControlsProviderMap;
        private ObservableMap<bool, FileManagerEventArgs<FileInformation>> _fileManagerMap;

        private readonly MultipleProvidersCache _multipleProvidersCache = new MultipleProvidersCache();

        private readonly SingleComparer<TimeSpan> _resyncComparer =
            new SingleComparer<TimeSpan>((span, subtitleInterval) => span > subtitleInterval);

        private static readonly object _padLock = new object();

        public SubtitleHandler([NotNull] Encoding encoding, [NotNull] ISubtitlesViewModel provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            _subtitleDisplayTracker = new IntermissionTimer();
            _subtitleDisplayTracker.Elapsed += SubtitleDisplayTracker_OnElapsed;
            InitializeMaps();
            ConfigureNotifications(provider);
        }

        private void InitializeMaps()
        {
            _subtitlesProviderMap = new MediaObservableMap<SubscriptionEventType>((map, args) => args.EventType)
            {
                {SubscriptionEventType.MediaAttached,
                    (sender, args) => AttachMediaElement((IMediaElementViewModel) args.AdditionalInfo)
                },
                {SubscriptionEventType.MediaDetached, (sender, args) => DetachMediaElement()},
            };

            _mediaElementProviderMap = new MediaObservableMap<MediaElementEventType>((map, args) => args.EventType)
            {
                {MediaElementEventType.Started, (sender, args) => TryBegin((MediaFileInformation) args.AdditionalInfo)},
                {MediaElementEventType.Ended, (sender, args) => Stop()}
            };

            _mediaControlsProviderMap = new MediaObservableMap<MediaControlEventType>(
                (map, args) => args.EventType,
                (map, type) => _subtitles != null)
            {
                {MediaControlEventType.FastForwardClick, SkipAsync},
                {MediaControlEventType.RewindClick, SkipAsync},
                {MediaControlEventType.PauseClick, (sender, args) => Pause()},
                {MediaControlEventType.PlayClick, (sender, args) => Resume()},
                {MediaControlEventType.StopClick, (sender, args) => Stop()},
                {MediaControlEventType.PositionSliderDragStarted, (sender, args) =>
                {
                    ForceHidingAllSubtitles();
                    Pause();
                }},
                {MediaControlEventType.PositionSliderDragCompleted, SkipAsync}
            };

            _fileManagerMap = new ObservableMap<bool, FileManagerEventArgs<FileInformation>>(
                (map, args) => args.SelectedFiles.FirstOrDefault() != null)
            {
                {
                    true, (sender, args) => Begin(args.SelectedFiles.First(),
                        _providerMediaElement?.MediaElement.Position ?? default(TimeSpan))
                }
            };
        }

        private void ConfigureNotifications(ISubtitlesViewModel subtitlesProvider)
        {
            _multipleProvidersCache.AddProvider(subtitlesProvider, _subtitlesProviderMap);
            _multipleProvidersCache.AddProvider(FileManager<FileInformation>.Instance, _fileManagerMap);
        }

        private void AttachMediaElement(IMediaElementViewModel mediaElementProvider)
        {
            _providerMediaElement = mediaElementProvider;
            _multipleProvidersCache.AddProvider(mediaElementProvider, _mediaElementProviderMap);
            if (mediaElementProvider.CurrentControls != null)
            {
                _multipleProvidersCache.AddProvider(mediaElementProvider.CurrentControls, _mediaControlsProviderMap);
            }
        }

        private void DetachMediaElement()
        {
            _multipleProvidersCache.RemoveProvider<IMediaElementViewModel, MediaEventArgs<MediaElementEventType>>();
            _multipleProvidersCache.RemoveProvider<IMediaControlsViewModel, MediaEventArgs<MediaControlEventType>>();
            _providerMediaElement = null;
            ForceHidingAllSubtitles();
        }

        private async void SubtitleDisplayTracker_OnElapsed(object sender, EventArgs e)
        {
            var lastSubtitle = _subtitles.Current;
            _subtitles.MoveNext();
            _subtitleDisplayTracker.Interval = _subtitles.Current.Interval.Start - lastSubtitle.Interval.Start;
            await Application.Current.Dispatcher.Invoke(async () => await ResyncWithPlayerAsync(lastSubtitle.Interval));
            await ShowSubtitleAsync(lastSubtitle, lastSubtitle.Interval.Duration);
        }

        private async Task ResyncWithPlayerAsync(SubtitleInterval currentInterval)
        {
            var mediaPosition = _providerMediaElement.MediaElement.Position;
            var result = currentInterval.Start.Subtract(mediaPosition);
            var tolerance = TimeSpan.FromMilliseconds(10);
            if (result > tolerance)
            {
                //current is ahead
                await Task.Delay(result, _pauseToken.Token).ContinueWith(_ => { });
            }
        }

        private void TryBegin(MediaFileInformation file)
        {
            if (Settings.DetectSubtitlesAutomatically)
            {
                var localSubs = SubtitleDetector.DetectSubtitles(file, Settings.PreferedSubtitleLanguage);
                if (localSubs != null)
                {
                    Begin(localSubs, TimeSpan.Zero);
                }
            }
        }

        private void Begin(IFileInformation subtitleFile, TimeSpan startTime)
        {
            if (_subtitles != null)
            {
                _subtitleDisplayTracker.Stop();
            }
            _subtitles = new SubtitleReader(_encoding).ExtractSubtitles(subtitleFile.Uri.OriginalString);
            Resync(startTime);
            _pauseToken = new CancellationTokenSource();
            _manualResetEventAsync.Set();
            _subtitleDisplayTracker.Start();
        }

        private void Pause()
        {
            _subtitleDisplayTracker.Pause();
            _manualResetEventAsync.Reset();
            _pauseToken.Cancel();
        }

        private void Resume()
        {
            _manualResetEventAsync.Set();
            _pauseToken = new CancellationTokenSource();
            _subtitleDisplayTracker.Resume();
        }

        private bool Resync(TimeSpan time)
        {
            ForceHidingAllSubtitles();
            _resyncComparer.TargetValue = time;
            var targetIndex =
                _subtitles.DuplicateBinarySearch(_resyncComparer, segment => segment.Interval.End);
            if (targetIndex == -1)
            {
                Stop();
                return false;
            }
            var targetSubs = _subtitles[targetIndex];

            _subtitles.SetCurrent(targetIndex);
            if (time > targetSubs.Interval.Start)
            {
                _subtitles.SetCurrent(targetIndex + 1);
#pragma warning disable 4014
                ShowSubtitleAsync(targetSubs, targetSubs.Interval.End.Subtract(time));
#pragma warning restore 4014
            }
            _subtitleDisplayTracker.Interval = _subtitles.Current.Interval.Start.Subtract(time);
            return true;
        }

        private async Task ResyncAndResumeAsync(TimeSpan span)
        {
            _pauseToken = new CancellationTokenSource();
            var resyncResult = true;
            await Task.Factory.StartNew(() =>
                    {
                        lock (_padLock)
                        {
                            resyncResult = Resync(span);
                        }
                    },
                    _pauseToken.Token,
                    TaskCreationOptions.RunContinuationsAsynchronously,
                    TaskScheduler.Current)
                .ContinueWith(task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion && resyncResult)
                    {
                        Resume();
                    }
                }, TaskScheduler.Current);
        }

        private async void SkipAsync(object sender, MediaEventArgs<MediaControlEventType> e)
        {
            var time = (TimeSpan)e.AdditionalInfo;
            if (_providerMediaElement.MediaPlayerModel.CurrentState.Equals(MediaPlayerStates.Play))
            {
                await ResyncAndResumeAsync(time);
            }
            else
            {
                Resync(time);
            }
        }

        private void Stop()
        {
            _subtitleDisplayTracker.Stop();
            Resync(TimeSpan.Zero);
        }

        private async Task ShowSubtitleAsync(SubtitleSegment subtitle, TimeSpan duration)
        {
            var clone = subtitle.SerializedClone();
            OnDisplaySubtitle(clone);
            _currentlyShownSubtitles.Add(clone);
            _forceHidingToken = new CancellationTokenSource();
            await ScheduleSubtitleHidingAsync(clone, duration);
        }

        private async Task ScheduleSubtitleHidingAsync(SubtitleSegment subtitle, TimeSpan span)
        {
            var startTime = DateTime.Now;
            await Task.Delay(span, _pauseToken.Token)
                .ContinueWith(task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        if (_currentlyShownSubtitles.Remove(subtitle))
                        {
                            OnHideSubtitle(subtitle);
                        }
                    }
                }, TaskScheduler.Current);

            var leftTime = span.Subtract(TimeSpan.FromTicks(DateTime.Now.Subtract(startTime).Ticks));
            if (leftTime > TimeSpan.FromMilliseconds(1))
            {
                await _manualResetEventAsync.WaitAsync();
                if (!_forceHidingToken.IsCancellationRequested)
                {
                    await ScheduleSubtitleHidingAsync(subtitle, leftTime);
                }
            }
        }

        private void ForceHidingAllSubtitles()
        {
            if (_currentlyShownSubtitles.Count == 0)
            {
                return;
            }
            _pauseToken.Cancel(false);
            _manualResetEventAsync.Reset();
            _forceHidingToken.Cancel(false);
            lock (_padLock)
            {
                foreach (var currentlyShownSubtitle in _currentlyShownSubtitles)
                {
                    OnHideSubtitle(currentlyShownSubtitle);
                }
                _currentlyShownSubtitles.Clear();
            }
        }

        private void OnHideSubtitle(SubtitleSegment subtitle)
        {
            OnNotify(SubtitlesEventType.Hide, subtitle);
        }

        private void OnDisplaySubtitle(SubtitleSegment subtitle)
        {
            OnNotify(SubtitlesEventType.Display, subtitle);
        }

        #region Implementation of ICustomObservable<MediaEventArgs<SubtitlesEventType>>

        public event EventHandler<MediaEventArgs<SubtitlesEventType>> Notify;

        private void OnNotify(SubtitlesEventType eventType, object additionalInfo = null)
        {
            Notify?.Invoke(this, new MediaEventArgs<SubtitlesEventType>(eventType, additionalInfo));
        }

        #endregion
    }
}
