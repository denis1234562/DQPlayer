using System;
using DQPlayer.Helpers;
using DQPlayer.Helpers.MediaEnumerations;
using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.SubtitlesManagement;

namespace DQPlayer.MVVMFiles.ViewModels
{
    public class SubtitlesViewModel : ISubtitlesViewModel
    {
        private MediaObservableMap<SubtitlesEventType> _subtitleMap;

        private IMediaElementViewModel _currentMediaElement;
        public IMediaElementViewModel CurrentMediaElement
        {
            get => _currentMediaElement;
            set
            {
                if (_currentMediaElement != null)
                {
                    OnSubscriptionNotify(SubscriptionEventType.MediaDetached, _currentMediaElement);
                }
                _currentMediaElement = value;
                OnSubscriptionNotify(SubscriptionEventType.MediaAttached, _currentMediaElement);
            }
        }

        public SubtitlesViewModel()
        {
            InitializeMaps();
            var subtitleHandler = new SubtitleHandler(Settings.Cyrillic, this);
            subtitleHandler.Notify += SubtitleHandler_OnNotify;
        }

        private void InitializeMaps()
        {
            _subtitleMap = new MediaObservableMap<SubtitlesEventType>
            ((map, args) => args.EventType)
            {
                [SubtitlesEventType.Display] =
                (sender, args) => OnSubtitlesNotify(SubtitlesEventType.Display, args.AdditionalInfo),
                [SubtitlesEventType.Hide] =
                (sender, args) => OnSubtitlesNotify(SubtitlesEventType.Hide, args.AdditionalInfo),
            };
        }

        private void SubtitleHandler_OnNotify(object sender, MediaEventArgs<SubtitlesEventType> e)
        {
            _subtitleMap[e.EventType].Invoke(sender, e);
        }

        #region Implementation of ICustomObservable<MediaEventArgs<SubscriptionEventType>>

        private event EventHandler<MediaEventArgs<SubscriptionEventType>> NotifySubscription;
        event EventHandler<MediaEventArgs<SubscriptionEventType>> ICustomObservable<MediaEventArgs<SubscriptionEventType>>.Notify
        {
            add => NotifySubscription += value;
            remove => NotifySubscription -= value;
        }

        private void OnSubscriptionNotify(SubscriptionEventType eventType, object additionalInfo = null)
        {
            NotifySubscription?.Invoke(this, new MediaEventArgs<SubscriptionEventType>(eventType, additionalInfo));
        }

        #endregion

        #region Implementation of ICustomObservable<MediaEventArgs<SubtitlesEventType>>

        private event EventHandler<MediaEventArgs<SubtitlesEventType>> NotifySubtitles;
        event EventHandler<MediaEventArgs<SubtitlesEventType>> ICustomObservable<MediaEventArgs<SubtitlesEventType>>.Notify
        {
            add => NotifySubtitles += value;
            remove => NotifySubtitles -= value;
        }

        private void OnSubtitlesNotify(SubtitlesEventType eventType, object additionalInfo = null)
        {
            NotifySubtitles?.Invoke(this, new MediaEventArgs<SubtitlesEventType>(eventType, additionalInfo));
        }

        #endregion
    }
}
