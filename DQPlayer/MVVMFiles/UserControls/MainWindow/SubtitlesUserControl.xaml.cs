using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DQPlayer.Helpers;
using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.CustomControls;
using DQPlayer.Helpers.MediaEnumerations;
using DQPlayer.Helpers.SubtitlesManagement;
using DQPlayer.MVVMFiles.ViewModels;

namespace DQPlayer.MVVMFiles.UserControls.MainWindow
{
    public partial class SubtitlesUserControl : UserControl
    {
        public SubtitlesViewModel ViewModel => DataContext as SubtitlesViewModel;

        private MediaObservableMap<SubtitlesEventType> _subtitlesMap;

        private readonly IEnumerable<OutlinedLabel> _subtitleLabels;

        public SubtitlesUserControl()
        {
            InitializeComponent();
            _subtitleLabels = gridSubtitles.Children.OfType<OutlinedLabel>();
            InitializeMaps();
            ((ICustomObservable<MediaEventArgs<SubtitlesEventType>>) ViewModel).Notify += SubtitlesUserControl_OnNotify;
        }

        private void InitializeMaps()
        {
            _subtitlesMap = new MediaObservableMap<SubtitlesEventType>((map, args) => args.EventType)
            {
                {SubtitlesEventType.Display, DisplaySubtitles},
                {SubtitlesEventType.Hide, HideSubtitles}
            };
        }

        private void HideSubtitles(object sender, MediaEventArgs<SubtitlesEventType> e)
        {
            Application.Current.Dispatcher.Invoke(
                () => _subtitleLabels.HideSubtitle((SubtitleSegment) e.AdditionalInfo));
        }

        private void DisplaySubtitles(object sender, MediaEventArgs<SubtitlesEventType> e)
        {
            Application.Current.Dispatcher.Invoke(
                () => _subtitleLabels.ShowSubtitle((SubtitleSegment) e.AdditionalInfo));
        }

        private void SubtitlesUserControl_OnNotify(object sender, MediaEventArgs<SubtitlesEventType> e)
        {
            _subtitlesMap[e.EventType].Invoke(sender, e);
        }
    }
}
