namespace DQPlayer.MVVMFiles.Models.MediaPlayer
{
    public interface IRegulatableMediaService : IMediaService
    {
        void Rewind();
        void FastForward();
        void SetNewPlayerPosition(System.TimeSpan newPosition);
    }
}