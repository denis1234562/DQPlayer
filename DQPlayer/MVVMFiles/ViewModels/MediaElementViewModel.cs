using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using DQPlayer.Annotations;
using DQPlayer.Helpers;
using DQPlayer.Helpers.CustomControls;
using DQPlayer.Helpers.FileManagement.FileInformation;
using DQPlayer.Helpers.InputManagement;
using DQPlayer.Helpers.MediaControls;
using DQPlayer.MVVMFiles.Commands;
using DQPlayer.MVVMFiles.Models.MediaPlayer;
using DQPlayer.States;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public class MediaElementViewModel : IMediaElementViewModel
    {
        private readonly Dictionary<MediaControlEventType, EventHandler<MediaControlEventArgs>> _handlers;

        private Action _currentFileCallback;
        private bool _repeatState;

        public event Action<object, MediaElement> MediaEnded;

        public RelayCommand<MediaElement> MediaEndedCommand { get; }       

        public MediaPlayerModel MediaPlayerModel { get; set; }

        public event Action<IMediaControlsViewModel> ControlsAttached;

        private IMediaControlsViewModel _curentControls;
        public IMediaControlsViewModel CurentControls
        {
            get => _curentControls;
            set
            {
                if (_curentControls != null)
                {
                    Unsubscribe(_curentControls);
                }
                _curentControls = value;
                Subscribe(_curentControls);
                OnControlsAttached(_curentControls);
                OnPropertyChanged();
            }
        }

        public MediaElementViewModel()
        {
            MediaEndedCommand = new RelayCommand<MediaElement>(OnMediaEnded);
            _handlers = new Dictionary<MediaControlEventType, EventHandler<MediaControlEventArgs>>
            {
                [MediaControlEventType.RewindClick] = (s, e) => MediaPlayerModel.MediaController.Rewind(),
                [MediaControlEventType.PlayClick] = (s, e) => MediaPlayerModel.SetMediaState(MediaPlayerStates.Play),
                [MediaControlEventType.PauseClick] = (s, e) => MediaPlayerModel.SetMediaState(MediaPlayerStates.Pause),
                [MediaControlEventType.FastForwardClick] = (s, e) => MediaPlayerModel.MediaController.FastForward(),
                [MediaControlEventType.StopClick] = (s, e) => MediaPlayerModel.SetMediaState(MediaPlayerStates.Stop),

                [MediaControlEventType.PositionSliderDragStarted] = OnPositionSliderDragStarted,
                [MediaControlEventType.PositionSliderDragCompleted] = (s, e) => MediaPlayerModel.ResumeSerializedState(),
                [MediaControlEventType.VolumeSliderValueChanged] = ControlsViewModel_VolumeSliderValueChanged,

                [MediaControlEventType.MoveNextClick] = OnMoveNextClick,
                [MediaControlEventType.MovePreviousClick] = OnMovePreviousClick,
                [MediaControlEventType.RepeatCheck] = (sender, args) => _repeatState = (bool) args.AdditionalInfo
            };
            FileManager<MediaFileInformation>.Instance.NewRequest += FileManager_OnNewRequest;
        }

        private void OnMoveNextClick(object s, MediaControlEventArgs e) => PlaylistManager.Instance.Request(this,
            new PlaylistManagerEventArgs(PlaylistAction.PlayNext));

        private void OnMovePreviousClick(object s, MediaControlEventArgs e) => PlaylistManager.Instance.Request(this,
            new PlaylistManagerEventArgs(PlaylistAction.PlayPrevious));

        private void FileManager_OnNewRequest(object sender, FileManagerEventArgs<MediaFileInformation> e)
        {
            if (!sender.Equals(this))
            {
                var firstFile = e.SelectedFiles.First();
                MediaPlayerModel.MediaController.SetNewPlayerSource(firstFile?.Uri);
                MediaPlayerModel.SetMediaState(MediaPlayerStates.None);
                if (firstFile != null)
                {
                    _currentFileCallback = () => e.Callback?.Invoke(firstFile, _repeatState);
                    MediaPlayerModel.SetMediaState(MediaPlayerStates.Play);
                }
            }   
        }

        private void OnControlsAttached(IMediaControlsViewModel controlsViewModel)
        {
            ControlsAttached?.Invoke(controlsViewModel);
        }

        private void ControlsViewModel_Notify(object sender, MediaControlEventArgs e)
        {
            if (_handlers.TryGetValue(e.EventType, out var action))
            {
                action.Invoke(sender, e);
            }
        }

        private void Subscribe(ICustomObservable<MediaControlEventArgs> provider)
        {
            provider.Notify += ControlsViewModel_Notify;
        }

        private void Unsubscribe(ICustomObservable<MediaControlEventArgs> provider)
        {
            provider.Notify -= ControlsViewModel_Notify;
        }

        #region Media Controls Event Handlers

        private void OnPositionSliderDragStarted(object sender, MediaControlEventArgs e)
        {
            //force video update
            MediaPlayerModel.MediaController.SetNewPlayerPosition(
                ((ThumbDragSlider) e.AdditionalInfo).Value.Add(TimeSpan.FromMilliseconds(10)));
            MediaPlayerModel.SerializeState(MediaPlayerStates.Pause);
        }

        private void ControlsViewModel_VolumeSliderValueChanged(object sender, MediaControlEventArgs e)
        {
            var routedArgs = (RoutedPropertyChangedEventArgs<double>) e.AdditionalInfo;
            MediaPlayerModel.MediaController.ChangeVolume(routedArgs.NewValue);
        }

        private void OnMediaEnded(MediaElement mediaElement)
        {
            _currentFileCallback?.Invoke();
            MediaEnded?.Invoke(this, mediaElement);
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
    }
}
