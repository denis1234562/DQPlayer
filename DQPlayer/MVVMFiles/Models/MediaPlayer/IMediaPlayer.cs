using DQPlayer.States;

namespace DQPlayer.MVVMFiles.Models.MediaPlayer
{
    public interface IMediaPlayer<out TService, TState>
        where TService : IMediaService
        where TState : BaseState
    {
        TService MediaController { get; }
        TState CurrentState { get; }
        void SetMediaState(TState state);
    }
}