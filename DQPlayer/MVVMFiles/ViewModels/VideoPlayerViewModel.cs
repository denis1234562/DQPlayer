using System;
using System.ComponentModel;
using System.Linq;
using DQPlayer.Annotations;
using DQPlayer.Extensions;
using DQPlayer.MVVMFiles.Commands;
using DQPlayer.MVVMFiles.Models.MediaPlayer;
using DQPlayer.States;
using Microsoft.Win32;
using DQPlayer.Helpers;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DQPlayer.ResourceFiles;
using System.Collections.Generic;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public class VideoPlayerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<IMediaService> Loaded;
        public event Action<IEnumerable<Uri>> MediaFileDropped;
        public event Action<Uri> MediaFileBrowsed;

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

        private readonly Lazy<RelayCommand> _mediaEndedCommand;
        public RelayCommand MediaEndedCommand => _mediaEndedCommand.Value;

        private readonly PlayListViewModel playListViewModel;

        public WindowService WindowService;

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
            _browseCommand = CreateLazyRelayCommand(OnBrowseCommand);
            _sliderDragStartedCommand = CreateLazyRelayCommand(() => MediaPlayer.SerializeState(MediaPlayerStates.Pause));
            _sliderDragCompletedCommand = CreateLazyRelayCommand(() => MediaPlayer.ResumeSerializedState());
            _thumbDragDeltaCommand = CreateLazyRelayCommand(() => MediaPlayer.SetPlayerPositionToCursor());
            _thumbMouseEnterCommand = CreateLazyRelayCommand<MouseEventArgs>(OnThumbMouseEnterCommand);
            _windowFileDropCommand = CreateLazyRelayCommand<DragEventArgs>(OnWindowFileDropCommand);
            _playListCommand = CreateLazyRelayCommand(OnPlayListCommand);
            _mediaEndedCommand = CreateLazyRelayCommand(OnMediaEnded);
            playListViewModel = new PlayListViewModel(this);
            WindowService = new WindowService();
            WindowService.ShowWindow(playListViewModel);
        }

        public MediaPlayerModel MediaPlayer { get; set; }

        public bool PlayerSourceState => MediaPlayer?.CurrentState != null &&
                                           !MediaPlayer.CurrentState.Equals(MediaPlayerStates.None);

        private static Lazy<RelayCommand> CreateLazyRelayCommand(Action execute, Func<bool> canExecute = null)
            => new Lazy<RelayCommand>(() => new RelayCommand(execute, canExecute));

        private static Lazy<RelayCommand<T>> CreateLazyRelayCommand<T>(Action<T> execute, Func<T, bool> canExecute = null)
            => new Lazy<RelayCommand<T>>(() => new RelayCommand<T>(execute, canExecute));

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
                MediaPlayer.PlayNewPlayerSource(new Uri(fileDialog.FileName));
                OnMediaFileBrowsed(new Uri(fileDialog.FileName));
            }
        }

        private void OnMediaFileBrowsed(Uri uri)
        {
            MediaFileBrowsed?.Invoke(uri);
        }

        private void OnWindowFileDropCommand(DragEventArgs e)
        {
            if (FileDropHandler.TryExtractDroppedItemsUri(e, Settings.MediaPlayerExtensionPackage, out var uris))
            {
                MediaPlayer.PlayNewPlayerSource(uris.First());
                OnMediaFileDropped(uris);
                return;
            }
            MessageBox.Show($"{Strings.InvalidFileType}", "Error");
        }

        private void OnMediaFileDropped(IEnumerable<Uri> uris)
        {
            MediaFileDropped?.Invoke(uris);
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
            if (WindowService.PlayListView.Visibility == Visibility.Hidden)
            {
                WindowService.PlayListView.Show();
            }
            else
            {
                WindowService.PlayListView.Hide();
            }
        }
        ///TO DO
        private void OnMediaEnded()
        {
           MediaPlayer.SetMediaState(MediaPlayerStates.Stop);
        }
    }
}