using System;
using DQPlayer.MVVMFiles.Models.MediaPlayer;

namespace DQPlayer.States
{
    [Serializable]
    public sealed class MediaPlayerState : BaseState
    {
        public bool IsRunning { get; }

        private readonly Action<IRegulatableMediaService> _mediaServiceAction;

        private MediaPlayerState(int id, bool isRunning) : base(id)
        {
            IsRunning = isRunning;
        }

        private MediaPlayerState(int id, bool isRunning, Action<IRegulatableMediaService> mediaServiceAction)
            : base(id)
        {
            IsRunning = isRunning;
            _mediaServiceAction = mediaServiceAction;
        }

        public void StateAction(IRegulatableMediaPlayer player)
        {
            _mediaServiceAction?.Invoke(player.MediaController);
        }
    }
}