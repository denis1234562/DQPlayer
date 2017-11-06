using System.ComponentModel;
using DQPlayer.Annotations;
using DQPlayer.CustomControls;
using DQPlayer.States;

namespace DQPlayer.MVVMFiles.Models.MediaPlayer
{
    public class MediaPlayerModel : INotifyPropertyChanged, IRegulatableMediaPlayer
    {
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
            CurrentState.StateAction(this);
        }
    }
}