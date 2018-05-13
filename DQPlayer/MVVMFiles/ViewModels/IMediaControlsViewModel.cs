using System.ComponentModel;
using DQPlayer.Helpers;
using DQPlayer.Helpers.MediaEnumerations;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public interface IMediaControlsViewModel : INotifyPropertyChanged,
        ICustomObservable<MediaEventArgs<MediaControlEventType>>
    {
        IMediaElementViewModel CurrentMediaPlayer { get; set; }
    }
}
