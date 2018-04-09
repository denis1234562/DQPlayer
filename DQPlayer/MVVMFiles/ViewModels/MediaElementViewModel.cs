using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using DQPlayer.Annotations;
using DQPlayer.Helpers;
using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.CustomControls;
using DQPlayer.Helpers.FileManagement.FileInformation;
using DQPlayer.Helpers.InputManagement;
using DQPlayer.Helpers.MediaEnumerations;
using DQPlayer.MVVMFiles.Commands;
using DQPlayer.MVVMFiles.Models.MediaPlayer;
using DQPlayer.States;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public class MediaElementViewModel : IMediaElementViewModel
    {
        private readonly MediaObservableMap<MediaControlEventType> _handlers;

        private Action _currentFileCallback;
        private bool _repeatState;

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
            _handlers = new MediaObservableMap<MediaControlEventType>((map, args) => args.EventType)
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
            FileManager<MediaFileInformation>.Instance.Notify += FileManager_OnNewRequest;
        }

        private void OnMoveNextClick(object s, MediaEventArgs<MediaControlEventType> e) => PlaylistManager.Instance
            .Request(this, new PlaylistManagerEventArgs(PlaylistAction.PlayNext));

        private void OnMovePreviousClick(object s, MediaEventArgs<MediaControlEventType> e) => PlaylistManager.Instance
            .Request(this, new PlaylistManagerEventArgs(PlaylistAction.PlayPrevious));

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
                    OnNotify(MediaElementEventType.Started, firstFile);
                }
            }
        }

        private void OnControlsAttached(IMediaControlsViewModel controlsViewModel)
        {
            ControlsAttached?.Invoke(controlsViewModel);
        }

        private void ControlsViewModel_Notify(object sender, MediaEventArgs<MediaControlEventType> e)
        {
            if (_handlers.TryGetValue(e.EventType, out var action))
            {
                action.Invoke(sender, e);
            }
        }

        private void Subscribe(ICustomObservable<MediaEventArgs<MediaControlEventType>> provider)
        {
            provider.Notify += ControlsViewModel_Notify;
        }

        private void Unsubscribe(ICustomObservable<MediaEventArgs<MediaControlEventType>> provider)
        {
            provider.Notify -= ControlsViewModel_Notify;
        }

        #region Media Controls Event Handlers

        private void OnPositionSliderDragStarted(object sender, MediaEventArgs<MediaControlEventType> e)
        {
            //force video update
            MediaPlayerModel.MediaController.SetNewPlayerPosition(
                ((ThumbDragSlider) e.AdditionalInfo).Value.Add(TimeSpan.FromMilliseconds(10)));
            MediaPlayerModel.SerializeState(MediaPlayerStates.Pause);
        }

        private void ControlsViewModel_VolumeSliderValueChanged(object sender, MediaEventArgs<MediaControlEventType> e)
        {
            var routedArgs = (RoutedPropertyChangedEventArgs<double>) e.AdditionalInfo;
            MediaPlayerModel.MediaController.ChangeVolume(routedArgs.NewValue);
        }

        private void OnMediaEnded(MediaElement mediaElement)
        {
            _currentFileCallback?.Invoke();
            OnNotify(MediaElementEventType.Ended);
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

        #region Implementation of ICustomObservable<MediaEventArgs<MediaElementEventType>>

        public event EventHandler<MediaEventArgs<MediaElementEventType>> Notify;

        private void OnNotify([NotNull] MediaElementEventType eventType, object additionalInfo = null)
        {
            if (eventType == null)
            {
                throw new ArgumentNullException(nameof(eventType));
            }
            Notify?.Invoke(this, new MediaEventArgs<MediaElementEventType>(eventType, additionalInfo));
        }

        #endregion
    }
}
