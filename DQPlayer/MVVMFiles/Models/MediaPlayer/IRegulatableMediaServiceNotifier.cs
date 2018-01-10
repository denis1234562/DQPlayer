using System;

namespace DQPlayer.MVVMFiles.Models.MediaPlayer
{
    public interface IRegulatableMediaServiceNotifier : IMediaServiceNotifier
    {
        event Action<object, TimeSpan> MediaPositionChanged;
    }
}