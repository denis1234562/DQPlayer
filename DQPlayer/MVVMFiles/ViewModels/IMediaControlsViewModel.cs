using System.ComponentModel;
using DQPlayer.Helpers;
using DQPlayer.Helpers.MediaControls;
using DQPlayer.MVVMFiles.UserControls.MainWindow;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public interface IMediaControlsViewModel : INotifyPropertyChanged, ICustomObservable<MediaControlEventArgs>
    {
        IMediaElementUserControl CurrentMediaPlayer { get; set; }
    }
}
