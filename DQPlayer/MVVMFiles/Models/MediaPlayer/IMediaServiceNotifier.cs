using System;

namespace DQPlayer.MVVMFiles.Models.MediaPlayer
{
    public interface IMediaServiceNotifier
    {
        event Action<object> MediaPlayed;
        event Action<object> MediaPaused;
        event Action<object> MediaStopped;
    }
}