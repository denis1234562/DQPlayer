using DQPlayer.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DQPlayer
{
    public enum TimerState
    {
        NotRunning,
        Paused,
        Running
    }

    public class StatefulIntermissionTimer : IntermissionTimer, INotifyPropertyChanged
    {
        private TimerState _currentState;
        public event PropertyChangedEventHandler PropertyChanged;

        public TimerState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                OnPropertyChanged();
            }
        }

        public override void Start()
        {
            base.Start();
            CurrentState = TimerState.Running;
        }

        public override void Stop()
        {
            base.Stop();
            CurrentState = TimerState.NotRunning;
        }

        public override void Pause()
        {
            base.Pause();
            CurrentState = TimerState.Paused;
        }

        public override void Resume()
        {
            base.Resume();
            CurrentState = TimerState.Running;
        }

        #region Implementation of INotifyPropertyChanged

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}