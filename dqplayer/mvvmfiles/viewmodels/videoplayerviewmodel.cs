using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DQPlayer.MVVMFiles.Commands;
using DQPlayer.MVVMFiles.Models.MediaPlayer;
using DQPlayer.States;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.Extensions;
using DQPlayer.Helpers.FileManagement;
using DQPlayer.Helpers.SubtitlesManagement;
using DQPlayer.Properties;
using DQPlayer.ResourceFiles;
using DQPlayer.MVVMFiles.Models.PlayList;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public class VideoPlayerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<IMediaService> Loaded;
        public event Action<IEnumerable<FileInformation>> MediaInputNewFiles;
        public event Action<FileInformation> MediaPlayedNewSource;

        private readonly Lazy<RelayCommand> _loadedCommand;
        public RelayCommand LoadedCommand => _loadedCommand.Value;

        private readonly Lazy<RelayCommand> _playCommand;
        public RelayCommand PlayCommand => _playCommand.Value;

        private readonly Lazy<RelayCommand> _pauseCommand;
        public RelayCommand PauseCommand => _pauseCommand.Value;

        private readonly Lazy<RelayCommand> _stopCommand;
        public RelayCommand StopCommand => _stopCommand.Value;

        private readonly Lazy<RelayCommand> _rewindCommand;
        public RelayCommand RewindCommand => _rewindCommand.Value;

        private readonly Lazy<RelayCommand> _fastForwardCommand;
        public RelayCommand FastForwardCommand => _fastForwardCommand.Value;

        private readonly Lazy<RelayCommand> _browseCommand;
        public RelayCommand BrowseCommand => _browseCommand.Value;

        private readonly Lazy<RelayCommand> _sliderDragStartedCommand;
        public RelayCommand SliderDragStartedCommand => _sliderDragStartedCommand.Value;

        private readonly Lazy<RelayCommand> _sliderDragCompletedCommand;
        public RelayCommand SliderDragCompletedCommand => _sliderDragCompletedCommand.Value;

        private readonly Lazy<RelayCommand> _thumbDragDeltaCommand;
        public RelayCommand ThumbDragDeltaCommand => _thumbDragDeltaCommand.Value;

        private readonly Lazy<RelayCommand<MouseEventArgs>> _thumbMouseEnterCommand;
        public RelayCommand<MouseEventArgs> ThumbMouseEnterCommand => _thumbMouseEnterCommand.Value;

        private readonly Lazy<RelayCommand<DragEventArgs>> _windowFileDropCommand;
        public RelayCommand<DragEventArgs> WindowFileDropCommand => _windowFileDropCommand.Value;

        private readonly Lazy<RelayCommand> _playListCommand;
        public RelayCommand PlayListCommand => _playListCommand.Value;

        private readonly Lazy<RelayCommand<Func<ObservableCircularList<FileInformation>, FileInformation>>> _moveNextCommand;
        public RelayCommand<Func<ObservableCircularList<FileInformation>, FileInformation>> MoveNextCommand => _moveNextCommand.Value;

        private readonly Lazy<RelayCommand<Func<ObservableCircularList<FileInformation>, FileInformation>>> _movePreviousCommand;
        public RelayCommand<Func<ObservableCircularList<FileInformation>, FileInformation>> MovePreviousCommand => _movePreviousCommand.Value;

        private readonly Lazy<RelayCommand> _mediaEndedCommand;
        public RelayCommand MediaEndedCommand => _mediaEndedCommand.Value;

        public VideoPlayerViewModel()
        {
            _loadedCommand = CreateLazyRelayCommand(OnLoadedCommand);
            _stopCommand = CreateLazyRelayCommand(() => MediaPlayer.SetMediaState(MediaPlayerStates.Stop));
            _pauseCommand = CreateLazyRelayCommand(() => MediaPlayer.SetMediaState(MediaPlayerStates.Pause));
            _playCommand = CreateLazyRelayCommand(() => MediaPlayer.SetMediaState(MediaPlayerStates.Play));
            _rewindCommand = CreateLazyRelayCommand(() =>
            {
                var lastState = MediaPlayer.CurrentState.SerializedClone();
                MediaPlayer.SetMediaState(MediaPlayerStates.Rewind);
                MediaPlayer.SetMediaState(lastState);
            });
            _fastForwardCommand = CreateLazyRelayCommand(() =>
            {
                var lastState = MediaPlayer.CurrentState.SerializedClone();
                MediaPlayer.SetMediaState(MediaPlayerStates.FastForward);
                MediaPlayer.SetMediaState(lastState);
            });
            _sliderDragStartedCommand = CreateLazyRelayCommand(() => MediaPlayer.SerializeState(MediaPlayerStates.Pause));
            _sliderDragCompletedCommand = CreateLazyRelayCommand(() => MediaPlayer.ResumeSerializedState());
            _thumbDragDeltaCommand = CreateLazyRelayCommand(() => MediaPlayer.SetPlayerPositionToCursor());
            _thumbMouseEnterCommand = CreateLazyRelayCommand<MouseEventArgs>(OnThumbMouseEnterCommand);
            _windowFileDropCommand = CreateLazyRelayCommand<DragEventArgs>(OnWindowFileDropCommand);
            _browseCommand = CreateLazyRelayCommand(OnBrowseCommand);
            _playListCommand = CreateLazyRelayCommand(OnPlayListCommand);
            _moveNextCommand = CreateLazyRelayCommand<Func<ObservableCircularList<FileInformation>, FileInformation>>(
                func => OnMoveCommand(list => list.MoveNext()));
            _movePreviousCommand = CreateLazyRelayCommand<Func<ObservableCircularList<FileInformation>, FileInformation>>(
                func => OnMoveCommand(list => list.MovePrevious()));
            _mediaEndedCommand = CreateLazyRelayCommand(OnMediaEndedCommand);

            //Replace
            PlayListViewModel = new PlayListViewModel(this);
            PlayListView = new Views.PlayList { DataContext = PlayListViewModel };
        }

        public PlayListViewModel PlayListViewModel { get; }
        public Views.PlayList PlayListView;

        private bool _repeat;
        public bool Repeat
        {
            get => _repeat;
            set
            {
                _repeat = value;
                OnPropertyChanged();
            }
        }

        public MediaPlayerModel MediaPlayer { get; set; }

        public bool PlayerSourceState => MediaPlayer?.CurrentState != null &&
                                         !MediaPlayer.CurrentState.Equals(MediaPlayerStates.None);

        private static Lazy<RelayCommand> CreateLazyRelayCommand(Action execute, Func<bool> canExecute = null)
            => new Lazy<RelayCommand>(() => new RelayCommand(execute, canExecute));

        private static Lazy<RelayCommand<T>> CreateLazyRelayCommand<T>(Action<T> execute, Func<T, bool> canExecute = null)
            => new Lazy<RelayCommand<T>>(() => new RelayCommand<T>(execute, canExecute));

        public event Action<SubtitleHandler, SubtitleSegment> Show;
        public event Action<SubtitleHandler, SubtitleSegment> Hide;

        private SubtitleHandler _currentSubtitleHandler;

        private readonly Dictionary<FileInformation, FileInfo> _subCache = new Dictionary<FileInformation, FileInfo>();

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void MediaPlayerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MediaPlayer.CurrentState))
            {
                OnPropertyChanged(nameof(PlayerSourceState));
                //TODO replace this with proper binding maybe?
                OnPropertyChanged(nameof(MediaPlayer));
            }
        }

        private void OnLoadedCommand()
        {
            MediaPlayer.PropertyChanged += MediaPlayerOnPropertyChanged;
            Loaded?.Invoke(MediaPlayer.MediaController);
        }

        private void OnBrowseCommand()
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = Settings.MediaPlayerExtensionPackageFilter.Filter,
                Multiselect = true
            };
            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                ProcessInputFiles(fileDialog.FileNames.Select(f => new FileInformation(f)));
            }
        }

        private void OnWindowFileDropCommand(DragEventArgs e)
        {
            if (FileDropHandler.ExtractDroppedItemsUri(e, Settings.MediaPlayerExtensionPackage, out var uris))
            {
                ProcessInputFiles(uris);
                return;
            }
            MessageBox.Show($"{Strings.InvalidFileType}", "Error");
        }

        private void ProcessInputFiles(IEnumerable<FileInformation> files)
        {
            MediaInputNewFiles(files);

            DeattachCurrentSubtitlesIfAny();
            var firstElement = files.First();
            if (firstElement.FileInfo.Extension == Settings.SubtitleExtensionString)
            {
                AttachNewSubtitles(firstElement);
                return;
            }

            ChangeMediaPlayerSource(PlayListViewModel.FilesCollection.Current);
        }

        private void DeattachCurrentSubtitlesIfAny()
        {
            if (_currentSubtitleHandler != null)
            {
                _currentSubtitleHandler.ForceHidingAllSubtitles();
                _currentSubtitleHandler.DisplaySubtitle -= OnDisplaySubtitle;
                _currentSubtitleHandler.HideSubtitle -= OnHideSubtitle;
            }
        }

        private void AttachNewSubtitles(FileInformation subtitleFile)
        {
            AttachNewSubtitles(subtitleFile.FileInfo.FullName);
        }

        private void AttachNewSubtitles(string subtitleFile)
        {
            if (!Equals(MediaPlayer.CurrentState, MediaPlayerStates.None))
            {
                _currentSubtitleHandler = new SubtitleHandler();
                _currentSubtitleHandler.DisplaySubtitle += OnDisplaySubtitle;
                _currentSubtitleHandler.HideSubtitle += OnHideSubtitle;
                _currentSubtitleHandler.WithEncoding(Settings.Cyrillic)
                    .IsStartable(MediaPlayer.CurrentState.IsRunning).Build(subtitleFile,
                        MediaPlayer.MediaSlider.Value,
                        (IRegulatableMediaServiceNotifier)MediaPlayer.MediaController);
            }
        }

        private void OnHideSubtitle(SubtitleHandler handler, SubtitleSegment segment)
        {
            Hide?.Invoke(handler, segment);
        }

        private void OnDisplaySubtitle(SubtitleHandler handler, SubtitleSegment segment)
        {
            Show?.Invoke(handler, segment);
        }

        private void OnThumbMouseEnterCommand(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.MouseDevice.Captured == null)
            {
                var args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
                {
                    RoutedEvent = UIElement.MouseLeftButtonDownEvent
                };
                ((Thumb)e.Source).RaiseEvent(args);
            }
        }

        private void OnPlayListCommand()
        {
            if (PlayListView.Visibility != Visibility.Visible)
            {
                PlayListView.Show();
            }
            else
            {
                PlayListView.Hide();
            }

        }

        private void OnMoveCommand(Func<ObservableCircularList<FileInformation>, FileInformation> action)
        {
            if (PlayListViewModel.FilesCollection.Count > 1)
            {
                action(PlayListViewModel.FilesCollection);
                ChangeMediaPlayerSource(PlayListViewModel.FilesCollection.Current);
            }
        }

        public void ChangeMediaPlayerSource(FileInformation newSource)
        {
            MediaPlayer.PlayNewPlayerSource(new Uri(newSource.FileInfo.FullName));
            OnMediaPlayedNewSource(newSource);
            LookForSubtitles(newSource);
        }

        private void OnMediaPlayedNewSource(FileInformation file)
        {
            MediaPlayedNewSource?.Invoke(file);
        }

        private void LookForSubtitles(FileInformation file)
        {
            DeattachCurrentSubtitlesIfAny();
            if (file.FileInfo.Directory == null)
            {
                return;
            }
            if (_subCache.TryGetValue(file, out var subtitle))
            {
                AttachNewSubtitles(subtitle.FullName);
                return;
            }
            var availableSubtitles =
                file.FileInfo.Directory.GetFiles($"*{Settings.SubtitleExtensionString}", SearchOption.AllDirectories);
            foreach (var subs in availableSubtitles)
            {
                if (Path.GetFileNameWithoutExtension(subs.Name) == file.FileName)
                {
                    AttachNewSubtitles(subs.FullName);
                    _subCache.Add(file, subs);
                    return;
                }
            }
        }

        private void OnMediaEndedCommand()
        {
            MediaPlayer.SetMediaState(MediaPlayerStates.Pause);
            return;
            if (PlayListViewModel.FilesCollection.Last().Equals(PlayListViewModel.FilesCollection.Current) && !Repeat)
            {
                PlayListViewModel.FilesCollection.MoveNext();
                MediaPlayer.SetMediaState(MediaPlayerStates.Stop);
                return;
            }
            PlayListViewModel.FilesCollection.MoveNext();
        }
    }
}