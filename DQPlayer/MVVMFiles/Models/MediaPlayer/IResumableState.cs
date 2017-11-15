using DQPlayer.States;

namespace DQPlayer.MVVMFiles.Models.MediaPlayer
{
    public interface IResumableState<TState>
        where TState : BaseState
    {
        TState CurrentState { get; }
        void SerializeState(TState newState);
        void ResumeSerializedState();
    }
}