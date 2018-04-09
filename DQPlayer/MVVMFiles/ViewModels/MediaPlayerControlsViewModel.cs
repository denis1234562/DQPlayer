using System;
using System.Linq;
using System.Windows;
using DQPlayer.States;
using Microsoft.Win32;
using System.Windows.Input;
using DQPlayer.Annotations;
using System.ComponentModel;
using DQPlayer.MVVMFiles.Views;
using DQPlayer.MVVMFiles.Commands;
using DQPlayer.Helpers.DialogHelpers;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DQPlayer.Helpers.CustomControls;
using DQPlayer.Helpers.FileManagement;
using DQPlayer.Helpers.InputManagement;
using DQPlayer.MVVMFiles.UserControls.MainWindow;
using DQPlayer.Helpers.FileManagement.FileInformation;
using DQPlayer.Helpers.MediaEnumerations;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public partial class MediaPlayerControlsViewModel : IMediaControlsViewModel
    {
        public MediaPlayerControlsViewModel()
        {
            PlaylistClickCommand = new RelayCommand(OnPlaylistClick);
            RepeatCheckCommand = new RelayCommand<bool>(OnRepeatChecked);
            SettingsClickCommand = new RelayCommand(OnSettingsClicked);
            BrowseFilesCommand = new RelayCommand(OnBrowseFiles);
            PlayPauseClickCommand = new RelayCommand<bool>(OnPlayPauseClick);
            FastForwardClickCommand = new RelayCommand(OnFastForwardClick);
            RewindClickCommand = new RelayCommand(OnRewindClick);
            StopClickCommand = new RelayCommand<ThumbDragSlider>(OnStopClick);
            MoveNextCommand = new RelayCommand(OnMoveNext);
            MovePreviousCommand = new RelayCommand(OnMovePrevious);
            PositionSliderDragStartedCommand = new RelayCommand<ThumbDragSlider>(OnPositionSliderDragStarted);
            PositionSliderDragCompletedCommand = new RelayCommand<TimeSpan>(OnPositionSliderDragCompleted);
            PositionSliderThumbMouseEnterCommand = new RelayCommand<MouseEventArgs>(OnThumbMouseEnter);
            VolumeSliderValueChangedCommand = new RelayCommand<RoutedPropertyChangedEventArgs<double>>(OnVolumeSliderValueChanged);
        }

        #region Command implementations

        private void OnPlayPauseClick(bool state)
        {
            OnNotify(new MediaEventArgs<MediaControlEventType>(state
                ? MediaControlEventType.PlayClick
                : MediaControlEventType.PauseClick));
        }

        private void OnThumbMouseEnter(MouseEventArgs e)
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

        private void OnPositionSliderDragCompleted(TimeSpan newPosition)
            => OnNotify(new MediaEventArgs<MediaControlEventType>(
                MediaControlEventType.PositionSliderDragCompleted,
                newPosition));

        private void OnPositionSliderDragStarted(ThumbDragSlider slider)
            => OnNotify(new MediaEventArgs<MediaControlEventType>(MediaControlEventType.PositionSliderDragStarted, slider));

        private void OnVolumeSliderValueChanged(RoutedPropertyChangedEventArgs<double> e) 
            => OnNotify(new MediaEventArgs<MediaControlEventType>(MediaControlEventType.VolumeSliderValueChanged, e));

        private void OnMoveNext() 
            => OnNotify(new MediaEventArgs<MediaControlEventType>(MediaControlEventType.MoveNextClick));

        private void OnMovePrevious() 
            => OnNotify(new MediaEventArgs<MediaControlEventType>(MediaControlEventType.MovePreviousClick));

        private void OnPlaylistClick()
            => WindowDialogHelper<PlaylistView>.Instance.Show();

        private void OnRepeatChecked(bool state) 
            => OnNotify(new MediaEventArgs<MediaControlEventType>(MediaControlEventType.RepeatCheck, state));

        private void OnSettingsClicked()
            => WindowDialogHelper<SettingsView>.Instance.Show();

        private void OnRewindClick()
        {
            TimeSpan time = CurrentMediaPlayer.MediaElement.Position.Add(Settings.RewindSeconds);
            OnNotify(new MediaEventArgs<MediaControlEventType>(MediaControlEventType.RewindClick, time));
        }

        private void OnFastForwardClick()
        {
            TimeSpan time = CurrentMediaPlayer.MediaElement.Position.Add(Settings.FastForwardSeconds);
            OnNotify(new MediaEventArgs<MediaControlEventType>(MediaControlEventType.FastForwardClick, time));
        }

        private void OnStopClick(ThumbDragSlider slider)
        {
            slider.Value = TimeSpan.Zero;
            OnNotify(new MediaEventArgs<MediaControlEventType>(MediaControlEventType.StopClick));
            IsCheckedState = false;
        }

        private void OnMediaAttached()
        {
            FileManager<MediaFileInformation>.Instance.Notify += CurrentMediaPlayer_MediaPlayedNewSource;
            MediaAttached?.Invoke(this, CurrentMediaPlayer);
        }

        private void OnMediaDetached()
        {
            FileManager<MediaFileInformation>.Instance.Notify -= CurrentMediaPlayer_MediaPlayedNewSource;
        }

        private void CurrentMediaPlayer_MediaPlayedNewSource(object sender, FileManagerEventArgs<MediaFileInformation> e)
        {
            IsCheckedState = e.SelectedFiles.First() != null;
            OnPropertyChanged(nameof(PlayerSourceState));
        }

        private void OnBrowseFiles()
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = Settings.MediaPlayerExtensionPackageFilter.Filter,
                Multiselect = true
            };
            if (fileDialog.ShowDialog().GetValueOrDefault())
            {
                var files = fileDialog.FileNames.Select(FileProcesser.Selector);
                FileManagerHelper.Request(this, files);
                OnNotify(new MediaEventArgs<MediaControlEventType>(MediaControlEventType.BrowseClick, files));
            }
        }

        private void TooltipUpdate(object[] values)
        {
            var tooltip = (ToolTip)values[0];
            var obtainValue = (Func<object>)values[1];

            tooltip.Content = obtainValue.Invoke();
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Implementation of ICustomObservable<MediaControlEventArgs>

        public event EventHandler<MediaEventArgs<MediaControlEventType>> Notify;

        protected virtual void OnNotify(MediaEventArgs<MediaControlEventType> eventArgs)
        {
            Notify?.Invoke(this, eventArgs);
        }

        #endregion
    }

    public partial class MediaPlayerControlsViewModel
    {
        public event Action<object, IMediaElementUserControl> MediaAttached;

        private IMediaElementUserControl _currentMediaPlayer;
        public IMediaElementUserControl CurrentMediaPlayer
        {
            get => _currentMediaPlayer;
            set
            {
                if (_currentMediaPlayer != null)
                {
                    OnMediaDetached();
                }
                _currentMediaPlayer = value;
                OnMediaAttached();
                OnPropertyChanged();
            }
        }

        #region Binding properties

        private bool _isCheckedState;
        public bool IsCheckedState
        {
            get => _isCheckedState;
            set
            {
                _isCheckedState = value;
                OnPropertyChanged();
            }
        }

        public bool PlayerSourceState
            => CurrentMediaPlayer?.MediaPlayerModel?.CurrentState != null &&
               !CurrentMediaPlayer.MediaPlayerModel.CurrentState.Equals(
                   MediaPlayerStates.None);

        #endregion

        #region Command definitions

        public RelayCommand PlaylistClickCommand { get; }
        public RelayCommand<bool> RepeatCheckCommand { get; }
        public RelayCommand SettingsClickCommand { get; }
        public RelayCommand BrowseFilesCommand { get; }
        public RelayCommand<bool> PlayPauseClickCommand { get; }
        public RelayCommand<ThumbDragSlider> StopClickCommand { get; }
        public RelayCommand RewindClickCommand { get; }
        public RelayCommand FastForwardClickCommand { get; }
        public RelayCommand MoveNextCommand { get; }
        public RelayCommand MovePreviousCommand { get; }
        public RelayCommand<RoutedPropertyChangedEventArgs<double>> VolumeSliderValueChangedCommand { get; }
        public RelayCommand<ThumbDragSlider> PositionSliderDragStartedCommand { get; }
        public RelayCommand<TimeSpan> PositionSliderDragCompletedCommand { get; }
        public RelayCommand<MouseEventArgs> PositionSliderThumbMouseEnterCommand { get; }
        public MultiValueRelayCommand TooltipUpdateCommand => new MultiValueRelayCommand(TooltipUpdate);

        #endregion
    }
}
