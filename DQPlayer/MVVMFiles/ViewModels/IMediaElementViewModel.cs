using DQPlayer.Helpers;
using System.ComponentModel;
using DQPlayer.Helpers.MediaEnumerations;
using DQPlayer.MVVMFiles.Models.MediaPlayer;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public interface IMediaElementViewModel : INotifyPropertyChanged,
        ICustomObservable<MediaEventArgs<MediaElementEventType>>
    {
        IMediaControlsViewModel CurentControls { get; set; }
        MediaPlayerModel MediaPlayerModel { get; set; }
    }
}
