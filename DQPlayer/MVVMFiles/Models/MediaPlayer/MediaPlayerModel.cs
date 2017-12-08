using System;
using System.ComponentModel;
using DQPlayer.Helpers.CustomControls;
using DQPlayer.Helpers.Extensions;
using DQPlayer.Properties;
using DQPlayer.States;

namespace DQPlayer.MVVMFiles.Models.MediaPlayer
{
    public class MediaPlayerModel : INotifyPropertyChanged, IRegulatableMediaPlayer, IResumableState<MediaPlayerState>
    {
        private MediaPlayerState _lastState;

        public event PropertyChangedEventHandler PropertyChanged;

        public IRegulatableMediaService MediaController { get; set; }
        public IntermissionTimer MediaPlayerTimer { get; }
        public ThumbDragSlider MediaSlider { get; }

        private MediaPlayerState _currentState;
        public MediaPlayerState CurrentState
        {
            get => _currentState;
            protected set
            {
                if (!Equals(value, _currentState))
                {
                    _currentState = value;
                    OnPropertyChanged();
                }
            }
        }

        public MediaPlayerModel(MediaPlayerState state, ThumbDragSlider mediaSlider)
        {
            MediaPlayerTimer = new IntermissionTimer();
            CurrentState = state;
            MediaSlider = mediaSlider;
            MediaSlider.DragStarted += MediaSlider_DragStarted;
        }

        private void MediaSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            this.SetPlayerPositionToCursor();
        }

        public MediaPlayerModel(ThumbDragSlider mediaSlider)
            : this(MediaPlayerStates.None, mediaSlider)
        {
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public void SetMediaState(MediaPlayerState state)
        {
            CurrentState = state;
            CurrentState.StateAction(MediaController);
        }

        public void SerializeState(MediaPlayerState newState)
        {
            _lastState = CurrentState.SerializedClone();
            SetMediaState(newState);
        }

        public void ResumeSerializedState()
        {
            if (_lastState == null)
            {
                return;
            }
            SetMediaState(_lastState);
            _lastState = null;
        }
    }
}