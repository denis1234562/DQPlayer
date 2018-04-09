using System;
using System.Linq;
using System.Text;
using System.Threading;
using DQPlayer.Annotations;
using System.Threading.Tasks;
using System.Collections.Generic;
using DQPlayer.MVVMFiles.ViewModels;
using DQPlayer.Helpers.InputManagement;
using Microsoft.VisualStudio.Threading;
using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.Extensions.CollectionsExtensions;
using DQPlayer.Helpers.MediaEnumerations;
using DQPlayer.Helpers.FileManagement.FileInformation;

namespace DQPlayer.Helpers.SubtitlesManagement
{
    public sealed class SubtitleHandler : ICustomObservable<MediaEventArgs<SubtitlesEventType>>
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly AsyncManualResetEvent _manualResetEventAsync = new AsyncManualResetEvent();

        private CircularList<SubtitleSegment> _subtitles;
        private readonly HashSet<SubtitleSegment> _currentlyShownSubtitles = new HashSet<SubtitleSegment>();

        private readonly IntermissionTimer _subtitleDisplayTracker;

        private readonly Encoding _encoding;

        private IMediaControlsViewModel _providerControls;

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
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            _subtitleDisplayTracker = new IntermissionTimer();
            _subtitleDisplayTracker.Tick += SubtitleDisplayTracker_OnTick;
            InitializeMaps();
            ConfigureNotifications(provider);
        }

        private void InitializeMaps()
        {
            _subtitlesProviderMap = new MediaObservableMap<SubscriptionEventType>((map, args) => args.EventType)
            {
                [SubscriptionEventType.MediaAttached] =
                (sender, args) => AttachMediaElement((IMediaElementViewModel) args.AdditionalInfo),
                [SubscriptionEventType.MediaDetached] = (sender, args) => DetachMediaElement()
            };

            _mediaElementProviderMap = new MediaObservableMap<MediaElementEventType>((map, args) => args.EventType)
            {
                [MediaElementEventType.Started] =
                (sender, args) => TryBegin((MediaFileInformation) args.AdditionalInfo),
                [MediaElementEventType.Ended] = (sender, args) => Stop()
            };

            _mediaControlsProviderMap = new MediaObservableMap<MediaControlEventType>((map, args) => args.EventType)
            {
                [MediaControlEventType.FastForwardClick] = Skip,
                [MediaControlEventType.RewindClick] = Skip,
                [MediaControlEventType.PauseClick] = (sender, args) => Pause(),
                [MediaControlEventType.PlayClick] = (sender, args) => Resume(),
                [MediaControlEventType.StopClick] = (sender, args) => Stop(),
                [MediaControlEventType.PositionSliderDragStarted] = (sender, args) =>
                {
                    ForceHidingAllSubtitles();
                    Pause();
                },
                [MediaControlEventType.PositionSliderDragCompleted] =
                async (sender, args) => await ResyncAndResumeAsync((TimeSpan) args.AdditionalInfo)
            };

            _fileManagerMap = new ObservableMap<bool, FileManagerEventArgs<FileInformation>>(
                (map, args) => args.SelectedFiles.FirstOrDefault() != null)
            {
                [true] = (sender, args) => Begin(args.SelectedFiles.First(),
                    _providerControls?.CurrentMediaPlayer.MediaElement.Position ?? default(TimeSpan))
            };
        }

        private async void Skip(object sender, MediaEventArgs<MediaControlEventType> args)
        {
            var time = (TimeSpan) args.AdditionalInfo;
            if (_subtitleDisplayTracker.IsEnabled)
            {
                await ResyncAndResumeAsync(time);
            }
            else
            {
                Resync(time);
            }
        }

        private void ConfigureNotifications(ISubtitlesViewModel subtitlesProvider)
        {
            _multipleProvidersCache.AddProvider(subtitlesProvider, _subtitlesProviderMap);
            _multipleProvidersCache.AddProvider(FileManager<FileInformation>.Instance, _fileManagerMap);
        }

        private void AttachMediaElement(IMediaElementViewModel mediaElementProvider)
        {
            _providerControls = mediaElementProvider.CurentControls;
            _multipleProvidersCache.AddProvider(mediaElementProvider, _mediaElementProviderMap);
            _multipleProvidersCache.AddProvider(_providerControls, _mediaControlsProviderMap);
        }

        private void DetachMediaElement()
        {
            _multipleProvidersCache.RemoveProvider<IMediaElementViewModel, MediaEventArgs<MediaElementEventType>>();
            _multipleProvidersCache.RemoveProvider<IMediaControlsViewModel, MediaEventArgs<MediaControlEventType>>();
            _providerControls = null;
            ForceHidingAllSubtitles();
        }

        private async void SubtitleDisplayTracker_OnTick(object sender, EventArgs e)
        {
            SubtitleSegment lastSubtitle = _subtitles.Current;
            _subtitles.MoveNext();
            _subtitleDisplayTracker.Interval = _subtitles.Current.SubtitleInterval.Start -
                                               lastSubtitle.SubtitleInterval.Start;
            await ShowSubtitleAsync(lastSubtitle, lastSubtitle.SubtitleInterval.Duration);
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
            _cancellationToken = new CancellationTokenSource();
            _manualResetEventAsync.Set();
            _subtitleDisplayTracker.Start();
        }

        private void Pause()
        {
            if (_subtitles == null)
            {
                return;
            }
            _subtitleDisplayTracker.Pause();
            _manualResetEventAsync.Reset();
            _cancellationToken.Cancel();
        }

        private void Resume()
        {
            if (_subtitles == null)
            {
                return;
            }
            _manualResetEventAsync.Set();
            _cancellationToken = new CancellationTokenSource();
            _subtitleDisplayTracker.Resume();
        }

        private void Resync(TimeSpan time)
        {
            if (_subtitles == null)
            {
                return;
            }
            ForceHidingAllSubtitles();
            _resyncComparer.TargetValue = time;
            var targetIndex =
                _subtitles.DuplicateBinarySearch(_resyncComparer, segment => segment.SubtitleInterval.End);
            var targetSubs = _subtitles[targetIndex];

            _subtitles.SetCurrent(targetIndex);
            if (time > targetSubs.SubtitleInterval.Start)
            {
                _subtitles.SetCurrent(targetIndex + 1);
#pragma warning disable 4014
                ShowSubtitleAsync(targetSubs, targetSubs.SubtitleInterval.End.Subtract(time));
#pragma warning restore 4014
            }
            _subtitleDisplayTracker.Interval = _subtitles.Current.SubtitleInterval.Start.Subtract(time);
        }

        private async Task ResyncAndResumeAsync(TimeSpan span)
        {
            if (_subtitles == null)
            {
                return;
            }
            _cancellationToken = new CancellationTokenSource();
            await Task.Factory.StartNew(() =>
                    {
                        lock (_padLock)
                        {
                            Resync(span);
                        }
                    },
                    _cancellationToken.Token,
                    TaskCreationOptions.RunContinuationsAsynchronously,
                    TaskScheduler.FromCurrentSynchronizationContext())
                .ContinueWith(task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        Resume();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Stop()
        {
            if (_subtitles == null)
            {
                return;
            }
            _subtitleDisplayTracker.Stop();
            Resync(TimeSpan.Zero);
        }

        private async Task ShowSubtitleAsync(SubtitleSegment subtitle, TimeSpan duration)
        {
            if (_subtitles == null)
            {
                return;
            }
            OnDisplaySubtitle(subtitle);
            _currentlyShownSubtitles.Add(subtitle);

            await ScheduleSubtitleHidingAsync(subtitle, duration);
        }

        private async Task ScheduleSubtitleHidingAsync(SubtitleSegment subtitle, TimeSpan span)
        {
            if (_subtitles == null)
            {
                return;
            }
            var startTime = DateTime.Now;

            await Task.Delay(span, _cancellationToken.Token)
                .ContinueWith(task =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                    {
                        OnHideSubtitle(subtitle);
                        _currentlyShownSubtitles.Remove(subtitle);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());

            var leftTime = span.Subtract(TimeSpan.FromTicks(DateTime.Now.Subtract(startTime).Ticks));
            if (leftTime > TimeSpan.FromMilliseconds(1))
            {
                await _manualResetEventAsync.WaitAsync();
                await ScheduleSubtitleHidingAsync(subtitle, leftTime);
            }
        }

        private void ForceHidingAllSubtitles()
        {
            if (_subtitles == null || _currentlyShownSubtitles.Count == 0)
            {
                return;
            }
            _cancellationToken.Cancel(false);
            _manualResetEventAsync.Reset();
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
            if (_subtitles == null)
            {
                return;
            }
            OnNotify(SubtitlesEventType.Hide, subtitle);
        }

        private void OnDisplaySubtitle(SubtitleSegment subtitle)
        {
            if (_subtitles == null)
            {
                return;
            }
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
