using DQPlayer.Helpers.CustomControls;
using DQPlayer.States;

namespace DQPlayer.MVVMFiles.Models.MediaPlayer
{
    public interface IRegulatableMediaPlayer : IMediaPlayer<IRegulatableMediaService, MediaPlayerState>
    {
        IntermissionTimer MediaPlayerTimer { get; }
        ThumbDragSlider MediaSlider { get; }
    }
}