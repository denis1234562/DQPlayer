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

        public event Action<object, MediaElement> MediaEnded;

        public RelayCommand<MediaElement> MediaEndedCommand { get; }       

        public event Action<IMediaControlsViewModel> ControlsAttached;

        public MediaPlayerModel MediaPlayerModel { get; set; }

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
                [MediaControlEventType.RewindClick] = OnRewindClick,
                [MediaControlEventType.PlayClick] = (s, e) => MediaPlayerModel.SetMediaState(MediaPlayerStates.Play),
                [MediaControlEventType.PauseClick] = (s, e) => MediaPlayerModel.SetMediaState(MediaPlayerStates.Pause),
                [MediaControlEventType.FastForwardClick] = OnFastForwardClick,
                [MediaControlEventType.StopClick] = (s, e) => MediaPlayerModel.SetMediaState(MediaPlayerStates.Stop),

                [MediaControlEventType.PositionSliderDragStarted] = OnPositionSliderDragStarted,
                [MediaControlEventType.PositionSliderDragCompleted] = (s, e) => MediaPlayerModel.ResumeSerializedState(),

                [MediaControlEventType.VolumeSliderValueChanged] = ControlsViewModel_VolumeSliderValueChanged,
            };
            Manager<MediaFileInformation>.NewRequest += MediaSourceManager_OnNewRequest;
        }

        private void MediaSourceManager_OnNewRequest(object sender, ManagerEventArgs<MediaFileInformation> e)
        {
            if (!sender.Equals(this))
            {
                var firstFile = e.SelectedFiles.FirstOrDefault();
                MediaPlayerModel.MediaController.SetNewPlayerSource(firstFile?.Uri);
                MediaPlayerModel.SetMediaState(MediaPlayerStates.None);
                if (firstFile != null)
                {
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

        private void OnFastForwardClick(object sender, MediaControlEventArgs e)
        {
            MediaPlayerModel.SerializeState(MediaPlayerStates.FastForward);
            MediaPlayerModel.ResumeSerializedState();
        }

        private void OnRewindClick(object sender, MediaControlEventArgs mediaControlEventArgs)
        {
            MediaPlayerModel.SerializeState(MediaPlayerStates.Rewind);
            MediaPlayerModel.ResumeSerializedState();
        }

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
