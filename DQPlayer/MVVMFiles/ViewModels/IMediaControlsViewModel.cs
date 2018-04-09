using System.ComponentModel;
using DQPlayer.Helpers;
using DQPlayer.Helpers.MediaEnumerations;
using DQPlayer.MVVMFiles.UserControls.MainWindow;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public interface IMediaControlsViewModel : INotifyPropertyChanged,
        ICustomObservable<MediaEventArgs<MediaControlEventType>>
    {
        IMediaElementUserControl CurrentMediaPlayer { get; set; }
    }
}
