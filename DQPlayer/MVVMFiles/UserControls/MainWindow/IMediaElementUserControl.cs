using System.Windows.Controls;
using DQPlayer.MVVMFiles.Models.MediaPlayer;

namespace DQPlayer.MVVMFiles.UserControls.MainWindow
{
    public interface IMediaElementUserControl
    {
        MediaElement MediaElement { get; }
        MediaPlayerModel MediaPlayerModel { get; set; }
    }
}