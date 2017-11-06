using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using DQPlayer.Annotations;
using DQPlayer.CustomControls;
using DQPlayer.MVVMFiles.Commands;
using DQPlayer.MVVMFiles.Models.MediaPlayer;
using DQPlayer.States;

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

        public VideoPlayerViewModel()
        {
            _loadedCommand = new Lazy<RelayCommand>(
                () => new RelayCommand(OnLoadedCommand));
            _stopCommand = new Lazy<RelayCommand>(
                () => new RelayCommand(() => MediaPlayer.SetMediaState(MediaPlayerStates.Stop)));
            _pauseCommand = new Lazy<RelayCommand>(
                () => new RelayCommand(() => MediaPlayer.SetMediaState(MediaPlayerStates.Pause)));
            _playCommand = new Lazy<RelayCommand>(
                () => new RelayCommand(() => MediaPlayer.SetMediaState(MediaPlayerStates.Play)));
            _rewindCommand = new Lazy<RelayCommand>(
                () => new RelayCommand(() =>
                {
                    var lastState = MediaPlayer.CurrentState.SerializedClone();
                    MediaPlayer.SetMediaState(MediaPlayerStates.Rewind);
                    MediaPlayer.SetMediaState(lastState);
                }));
            _fastForwardCommand = new Lazy<RelayCommand>(
                () => new RelayCommand(() =>
                {
                    var lastState = MediaPlayer.CurrentState.SerializedClone();
                    MediaPlayer.SetMediaState(MediaPlayerStates.FastForward);
                    MediaPlayer.SetMediaState(lastState);
                }));
        }

        public MediaPlayerModel MediaPlayer { get; set; }

        public bool PlayerSourceState => MediaPlayer?.CurrentState != null &&
                                         !MediaPlayer.CurrentState.Equals(MediaPlayerStates.None);

        public ImageSource IsPlaying
        {
            get
            {
                if (MediaPlayer?.CurrentState != null && MediaPlayer.CurrentState.IsRunning)
                {
                    return Settings.PauseImage;
                }
                return Settings.PlayImage;
            }
        }

        private void OnLoadedCommand()
        {
            MediaPlayer.PropertyChanged += MediaPlayerOnPropertyChanged;
            Loaded?.Invoke(MediaPlayer.MediaController);
        }

        private void MediaPlayerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MediaPlayer.CurrentState))
            {
                OnPropertyChanged(nameof(PlayerSourceState));
                OnPropertyChanged(nameof(IsPlaying));
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}