using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DQPlayer.MVVMFiles.Commands;
using DQPlayer.MVVMFiles.Models.MediaPlayer;
using DQPlayer.States;
using Microsoft.Win32;
using DQPlayer.Helpers;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DQPlayer.Helpers.Extensions;
using DQPlayer.Helpers.FileManagement;
using DQPlayer.Helpers.SubtitlesManagement;
using DQPlayer.Properties;
using DQPlayer.ResourceFiles;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public class VideoPlayerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<IMediaService> Loaded;

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
                Filter = Settings.MediaPlayerExtensionPackageFilter.Filter
            };
            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                ProcessInputFiles(new Uri(fileDialog.FileName).AsEnumerable());
            }
        }

        private void OnWindowFileDropCommand(DragEventArgs e)
        {
            if (FileDropHandler.TryExtractDroppedItemsUri(e, Settings.MediaPlayerExtensionPackage, out var uris))
            {
                ProcessInputFiles(uris);
                return;
            }
            MessageBox.Show($"{Strings.InvalidFileType}", "Error");
        }

        private void ProcessInputFiles(IEnumerable<Uri> files)
        {
            if (_currentSubtitleHandler != null)
            {
                _currentSubtitleHandler.ForceHidingAllSubtitles();
                _currentSubtitleHandler.DisplaySubtitle -= OnDisplaySubtitle;
                _currentSubtitleHandler.HideSubtitle -= OnHideSubtitle;
            }
            var firstElement = files.First();
            if (firstElement.AbsolutePath.GetFileExtension() == ".srt")
            {
                if (!Equals(MediaPlayer.CurrentState, MediaPlayerStates.None))
                {
                    _currentSubtitleHandler = new SubtitleHandler();
                    _currentSubtitleHandler.DisplaySubtitle += OnDisplaySubtitle;
                    _currentSubtitleHandler.HideSubtitle += OnHideSubtitle;
                    _currentSubtitleHandler.WithEncoding(Settings.Cyrillic)
                        .IsStartable(MediaPlayer.CurrentState.IsRunning).Build(firstElement.AbsolutePath,
                            MediaPlayer.MediaSlider.Value,
                            (IRegulatableMediaServiceNotifier)MediaPlayer.MediaController);
                }
                else
                {
                    //load in playlist
                }
                return;
            }
            MediaPlayer.PlayNewPlayerSource(firstElement);
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
    }
}