using System;
using System.ComponentModel;
using System.Windows.Controls;
using DQPlayer.MVVMFiles.Models.MediaPlayer;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public interface IMediaElementViewModel : INotifyPropertyChanged
    {
        IMediaControlsViewModel CurentControls { get; set; }
        MediaPlayerModel MediaPlayerModel { get; set; }
        event Action<object, MediaElement> MediaEnded;
    }
}
