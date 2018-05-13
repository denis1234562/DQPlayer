using DQPlayer.Helpers;
using System.ComponentModel;
using System.Windows.Controls;
using DQPlayer.Helpers.MediaEnumerations;
using DQPlayer.MVVMFiles.Models.MediaPlayer;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public interface IMediaElementViewModel : INotifyPropertyChanged,
        ICustomObservable<MediaEventArgs<MediaElementEventType>>
    {
        IMediaControlsViewModel CurrentControls { get; set; }
        MediaPlayerModel MediaPlayerModel { get; set; }
        MediaElement MediaElement { get; }
    }
}
