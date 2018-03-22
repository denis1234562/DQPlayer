using System;
using System.ComponentModel;
using DQPlayer.Helpers.Extensions;
using DQPlayer.Properties;
using DQPlayer.States;

namespace DQPlayer.MVVMFiles.Models.MediaPlayer
{
    public class MediaPlayerModel : INotifyPropertyChanged, IRegulatableMediaPlayer, IResumableState<MediaPlayerState>
    {
        private MediaPlayerState _lastState;

        public IntermissionTimer MediaPlayerTimer { get; }

        public IRegulatableMediaService MediaController { get; set; }

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

        public MediaPlayerModel(MediaPlayerState state)
        {
            MediaPlayerTimer = new IntermissionTimer();
            CurrentState = state;
        }

        public void SetMediaState(MediaPlayerState state)
        {
            CurrentState = state;
            CurrentState.StateAction(MediaController);
        }

        #region Implementation of IResumableState<MediaPlayerState>

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

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion
    }
}