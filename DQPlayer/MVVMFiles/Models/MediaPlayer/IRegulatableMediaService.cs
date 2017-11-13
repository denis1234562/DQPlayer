using System;

namespace DQPlayer.MVVMFiles.Models.MediaPlayer
{
    public interface IRegulatableMediaService : IMediaService
    {
        void Rewind();
        void FastForward();
        void SetNewPlayerPosition(TimeSpan newPosition);
        void SetNewPlayerSource(Uri source);
    }
}