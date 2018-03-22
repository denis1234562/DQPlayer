using DQPlayer.States;

namespace DQPlayer.MVVMFiles.Models.MediaPlayer
{
    //TODO
    //get property with reflection based on generic argument
    //<Margin>, <MediaPlayerState>,...
    public interface IResumableState<TState>
        where TState : BaseState
    {
        TState CurrentState { get; }
        void SerializeState(TState newState);
        void ResumeSerializedState();
    }
}