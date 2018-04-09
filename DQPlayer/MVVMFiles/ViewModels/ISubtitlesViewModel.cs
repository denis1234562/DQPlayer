using DQPlayer.Helpers;
using DQPlayer.Helpers.MediaEnumerations;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public interface ISubtitlesViewModel : ICustomObservable<MediaEventArgs<SubscriptionEventType>>,
        ICustomObservable<MediaEventArgs<SubtitlesEventType>>
    {
        IMediaElementViewModel CurrentMediaElement { get; set; }
    }
}